using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.Genre;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.Repositories;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class GenreWindowViewModel : ViewModelWindowBase
{
    private readonly IGenreRepository myGenreRepository;
    private readonly IBookPartitionRepository myBookPartitionRepository;


    public GenreWindowViewModel(IGenreRepository genreRepository, ILayoutRepository layoutRepository, 
        IBookPartitionRepository bookPartitionRepository)
    {
        DataControl = new GenreView();
        myGenreRepository = genreRepository;
        myBookPartitionRepository = bookPartitionRepository;
        myLayoutRepository = layoutRepository;
        Properties.WindowTitle = "Список типов литературы";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.GenreMenuId;
        Properties.RightMenuBar =
        [
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                ToolTip = "Обновить",
                Command = DocumentRefreshCommand
            },
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                ToolTip = "Сохранить",
                Command = DocumentSaveCommand
            },
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                ToolTip = "Закрыть документ",
                Command = DocumentCloseCommand
            }
        ];
        AddSameLevelCommand = new DelegateCommand(
            AddSameLevelGenre,
            () => true);
        AddNextLevelCommand = new DelegateCommand(
            AddNextLevelGenre,
            () => CurrentGenre is not null);
        DeleteCommand = new DelegateCommand(
            DeleteGenre,
            () => CurrentGenre is not null && Genres.All(_ => _.ParentId != CurrentGenre.Id));
        MoveToParentLevelCommand =  new DelegateCommand(
            MoveToHighLevel,
            () => CurrentGenre is not null && CurrentGenre.ParentId is not null);
        LoadReferences();
    }

    #region Properties

    public ObservableCollection<GenreWrapper> Genres { set; get; } = [];
    public ObservableCollection<GenreWrapper> SelectedGenres { set; get; } = [];
    public ObservableCollection<GenreWrapper> DeletedGenres { set; get; } = [];

    public List<RefName> BookPartitions { set; get; } = [];

    public GenreWrapper CurrentGenre
    {
        get => GetValue<GenreWrapper>();
        set => SetValue(value);
    }

    #endregion


    #region Commands

    public ICommand AddSameLevelCommand { get; private set; }
    public ICommand AddNextLevelCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    public ICommand MoveToParentLevelCommand { get; private set; }

    private void MoveToHighLevel()
    {
        CurrentGenre.ParentId = null;
    }

    private void AddSameLevelGenre()
    {
        var newItem = new GenreWrapper(new Genre
        {
            _id = Guid.NewGuid(),
            Name = "Новый",
            ParentId = CurrentGenre.ParentId
        })
        {
            State = StateEnum.New
        };
        Genres.Add(newItem);
        CurrentGenre = newItem;
        //if (DataControl is Genres view)
        //{
        //    view.tableView.MoveLastRow();
        //    view.gridControl.CurrentColumn = view.gridControl.Columns[0];
        //}
    }

    private void AddNextLevelGenre()
    {
        var newItem = new GenreWrapper(new Genre
        {
            _id = Guid.NewGuid(),
            Name = "Новый",
            ParentId = CurrentGenre.Id,
            Partition = CurrentGenre?.Partition is not null ? new RefName
            {
                Id = CurrentGenre.Partition.Id,
                Name = CurrentGenre.Partition.Name
            } : null
        })
        {
            State = StateEnum.New
        };
        Genres.Add(newItem);
        CurrentGenre = newItem;
        if (DataControl is GenreView view)
        {
            var node = view.treeListView.GetNodeByContent(CurrentGenre);
            view.treeListView.FocusedNode = node;
            view.treeListView.ShowEditor(true);
        }
    }
    
    private void DeleteGenre()
    {
        DeletedGenres.Add(CurrentGenre);
        Genres.Remove(CurrentGenre);
    }

    public void LoadReferences()
    {
        BookPartitions.Clear();
        var parts = Task.Run(() => ((BaseRepository<BookPartition>)myBookPartitionRepository).GetAllAsync()).Result;
        if (parts is null) return;
        foreach (var cntr in parts.OrderBy(_ => _.Name))
            BookPartitions.Add(new RefName
            {
                Id = cntr._id,
                Name = cntr.Name
            });
    }

    public override async Task DocumentRefreshAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;

            var data = await ((BaseRepository<Genre>)myGenreRepository).GetAllAsync();
            Genres.Clear();
            if (data is not null)
                foreach (var auth in data.OrderBy(_ => _.Name))
                    Genres.Add(new GenreWrapper(auth)
                    {
                        State = StateEnum.NotChanged
                    });
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
        finally
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Hidden;
        }
    }

    public override bool CanDocumentSave => Genres != null && ((Genres.Count > 0 &&
                                                                Genres.Any(_ => _.State != StateEnum.NotChanged) &&
                                                                Genres.All(_ =>
                                                                    !string.IsNullOrWhiteSpace(_.Name))) ||
                                                               DeletedGenres.Any());

    public override async Task DocumentSaveAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;
            if (DeletedGenres.Any())
                foreach (var auth in DeletedGenres)
                    await ((BaseRepository<Genre>)myGenreRepository).DeleteAsync(auth.Model);

            foreach (var auth in Genres.Where(_ => _.State != StateEnum.NotChanged))
                switch (auth.State)
                {
                    case StateEnum.Changed:
                        await ((BaseRepository<Genre>)myGenreRepository).UpdateAsync(auth.Model);
                        break;
                    case StateEnum.New:
                        await ((BaseRepository<Genre>)myGenreRepository).CreateAsync(auth.Model);
                        break;
                }

            DeletedGenres.Clear();
            foreach (var auth in Genres) auth.State = StateEnum.NotChanged;
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
        finally
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Hidden;
        }
    }

    #endregion
}
