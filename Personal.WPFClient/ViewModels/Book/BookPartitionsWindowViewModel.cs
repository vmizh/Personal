using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.Book;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class BookPartitionsWindowViewModel : ViewModelWindowBase
{
    private readonly IBookPartitionRepository mybookPartRepository;

    public BookPartitionsWindowViewModel(IBookPartitionRepository bookPartRepository,
        ILayoutRepository layoutRepository)
    {
        DataControl = new BookPartitionView();
        mybookPartRepository = bookPartRepository;
        myLayoutRepository = layoutRepository;
        Properties.WindowTitle = "Список разделов";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.BookPartitionMenuId;
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
            AddBookPart,
            () => true);
        DeleteCommand = new DelegateCommand(
            DeleteBookPart,
            () => CurrentBookPart != null);
        LoadReferences();
    }

    #region Properties

    public ObservableCollection<BookPartWrapper> BookPartitions { set; get; } = [];
    public ObservableCollection<BookPartWrapper> SelectedBookParts { set; get; } = [];
    public ObservableCollection<BookPartWrapper> DeletedBookParts { set; get; } = [];


    public BookPartWrapper CurrentBookPart
    {
        get => GetValue<BookPartWrapper>();
        set => SetValue(value);
    }

    #endregion


    #region Commands

    public ICommand AddCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    private void AddBookPart()
    {
        var newItem = new BookPartWrapper(new BookPartition
        {
            _id = Guid.NewGuid(),
            Name = "Новый"
        })
        {
            State = StateEnum.New
        };
        BookPartitions.Add(newItem);
        CurrentBookPart = newItem;
        if (DataControl is BookPartitionView view)
        {
            view.TableView.MoveLastRow();
            view.GridControl.CurrentColumn = view.GridControl.Columns[0];
        }
    }

    private void DeleteBookPart()
    {
        DeletedBookParts.Add(CurrentBookPart);
        BookPartitions.Remove(CurrentBookPart);
        if (DataControl is BookPartitionView view)
        {
            view.GridControl.RefreshData();
        }
    }

    public void LoadReferences()
    {
    }

    public override async Task DocumentRefreshAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;

            var data = await ((BaseRepository<BookPartition>)mybookPartRepository).GetAllAsync();
            BookPartitions.Clear();
            if (data is not null)
                foreach (var bookPart in data.OrderBy(_ => _.Name))
                    BookPartitions.Add(new BookPartWrapper(bookPart)
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

    public override bool CanDocumentSave => BookPartitions != null && ((BookPartitions.Count > 0 &&
                                                                        BookPartitions.Any(_ =>
                                                                            _.State != StateEnum.NotChanged) &&
                                                                        BookPartitions.All(_ =>
                                                                            !string.IsNullOrWhiteSpace(_.Name))) ||
                                                                       DeletedBookParts.Any());

    public override async Task DocumentSaveAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;
            if (DeletedBookParts.Any())
                foreach (var bookPart in DeletedBookParts)
                    await ((BaseRepository<BookPartition>)mybookPartRepository).DeleteAsync(bookPart.Model);

            foreach (var item in BookPartitions.Where(_ => _.State != StateEnum.NotChanged))
                switch (item.State)
                {
                    case StateEnum.Changed:
                        await ((BaseRepository<BookPartition>)mybookPartRepository).UpdateAsync(item.Model);
                        break;
                    case StateEnum.New:
                        await ((BaseRepository<BookPartition>)mybookPartRepository).CreateAsync(item.Model);
                        break;
                }

            DeletedBookParts.Clear();
            foreach (var auth in BookPartitions) auth.State = StateEnum.NotChanged;
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
