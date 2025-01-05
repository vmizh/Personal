using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.Genre;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.Repositories;
using WPFCore.ViewModel;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class GenreSelectDialogViewModel : ViewModelDialogBase
{
    private readonly IGenreRepository myGenreRepository;

    public GenreSelectDialogViewModel(IGenreRepository genreRepository, ILayoutRepository layoutRepository)
    {
        myGenreRepository = genreRepository;
        myLayoutRepository = layoutRepository;
        Properties.Id = MenuAndDocumentIds.GenreSelectDialogId;
        CustomDataUserControl = new GenreSelectDialogView();
        try
        {
            var data = Task.Run(() => ((BaseRepository<Genre>)myGenreRepository).GetAllAsync()).Result;
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
        AddCurrentGenreCommand = new DelegateCommand(AddCurrentGenre,
            () => CurrentGenre is not null);
        DeleteSelectedGenreCommand = new DelegateCommand(DeleteSelectedGenre,
            () => CurrentSelectedGenre is not null);
    }

    private void AddCurrentGenre()
    {
        if(!SelectedGenres.Contains(CurrentGenre))
            SelectedGenres.Add(CurrentGenre);
    }

    private void DeleteSelectedGenre()
    {
        SelectedGenres.Remove(CurrentSelectedGenre);
    }

    #region Command

    public ICommand DeleteSelectedGenreCommand { get; private set; }
    public ICommand AddCurrentGenreCommand { get; private set; }

    #endregion

    public ObservableCollection<GenreWrapper> Genres { set; get; } = [];
    public ObservableCollection<GenreWrapper> SelectedGenres { set; get; } = [];
    public ObservableCollection<GenreWrapper> ActualSelectedGenres { set; get; } = [];

    public GenreWrapper CurrentGenre
    {
        get => GetValue<GenreWrapper>();
        set => SetValue(value);
    }

    public GenreWrapper CurrentSelectedGenre
    {
        get => GetValue<GenreWrapper>();
        set => SetValue(value);
    }
}
