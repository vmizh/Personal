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
using Personal.WPFClient.Views;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class AuthorsWindowViewModel : ViewModelWindowBase
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly ICountryRepository myCountryRepository;

    public AuthorsWindowViewModel(IAuthorRepository authorRepository, ICountryRepository countryRepository,
        ILayoutRepository layoutRepository)
    {
        DataControl = new Authors();
        myAuthorRepository = authorRepository;
        myCountryRepository = countryRepository;
        myLayoutRepository = layoutRepository;
        Properties.WindowTitle = "Список авторов";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.AuthorMenuId;
        Properties.LeftMenuBar =
        [
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuOptions"] as ControlTemplate,
                ToolTip = "Настройки",
                SubMenu =
                [
                    new MenuButtonInfo
                    {
                        Image = Application.Current.Resources["imageResetLayout"] as DrawingImage,
                        Caption = "Переустановить разметку",
                        Command = OnWindowResetLayoutCommand
                    }
                ]
            }
        ];
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
        AddCommand = new DelegateCommand(
            AddAuthor,
            () => true);
        DeleteCommand = new DelegateCommand(
            DeleteAuthor,
            () => CurrentAuthor != null);
        LoadReferences();
    }

    #region Properties

    public ObservableCollection<AuthorWrapper> Authors { set; get; } = [];
    public ObservableCollection<AuthorWrapper> SelectedAuthors { set; get; } = [];
    public ObservableCollection<AuthorWrapper> DeletedAuthors { set; get; } = [];

    public List<RefName> Countries { set; get; } = [];

    public AuthorWrapper CurrentAuthor
    {
        get => GetValue<AuthorWrapper>();
        set => SetValue(value);
    }

    #endregion


    #region Commands

    public ICommand AddCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    private void AddAuthor()
    {
        var newItem = new AuthorWrapper(new Author
        {
            _id = Guid.NewGuid(),
            LastName = "Новый"
        })
        {
            State = StateEnum.New
        };
        Authors.Add(newItem);
        CurrentAuthor = newItem;
        if (DataControl is Authors view)
        {
            view.tableView.MoveLastRow();
            view.gridControl.CurrentColumn = view.gridControl.Columns[0];
        }
    }

    private void DeleteAuthor()
    {
        DeletedAuthors.Add(CurrentAuthor);
        Authors.Remove(CurrentAuthor);
    }

    public void LoadReferences()
    {
        Countries.Clear();
        var countries = Task.Run(() => ((BaseRepository<Country>)myCountryRepository).GetAllAsync()).Result;
        if (countries is null) return;
        foreach (var cntr in countries.OrderBy(_ => _.Name))
            Countries.Add(new RefName
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

            var data = await ((BaseRepository<Author>)myAuthorRepository).GetAllAsync();
            Authors.Clear();
            if (data is not null)
                foreach (var auth in data.OrderBy(_ => _.Name))
                    Authors.Add(new AuthorWrapper(auth)
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

    public override bool CanDocumentSave => Authors != null && ((Authors.Count > 0 &&
                                                                 Authors.Any(_ => _.State != StateEnum.NotChanged) &&
                                                                 Authors.All(_ =>
                                                                     !string.IsNullOrWhiteSpace(_.Name))) ||
                                                                DeletedAuthors.Any());

    public override async Task DocumentSaveAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;
            if (DeletedAuthors.Any())
                foreach (var auth in DeletedAuthors)
                    await ((BaseRepository<Author>)myAuthorRepository).DeleteAsync(auth.Model);

            foreach (var auth in Authors.Where(_ => _.State != StateEnum.NotChanged))
                switch (auth.State)
                {
                    case StateEnum.Changed:
                        await ((BaseRepository<Author>)myAuthorRepository).UpdateAsync(auth.Model);
                        break;
                    case StateEnum.New:
                        await ((BaseRepository<Author>)myAuthorRepository).CreateAsync(auth.Model);
                        break;
                }

            DeletedAuthors.Clear();
            foreach (var auth in Authors) auth.State = StateEnum.NotChanged;
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
