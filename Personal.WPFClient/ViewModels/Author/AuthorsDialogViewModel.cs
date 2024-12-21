using Personal.WPFClient.Repositories;
using System.Collections.ObjectModel;
using Personal.WPFClient.Views.Author;
using WPFCore.ViewModel;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Wrappers.Base;
using System;
using System.Linq;
using System.Threading.Tasks;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class AuthorsDialogViewModel : ViewModelDialogBase
{
    private readonly IAuthorRepository myAuthorRepository;

    public AuthorsDialogViewModel(IAuthorRepository authorRepository)
    {
        myAuthorRepository = authorRepository;
        CustomDataUserControl = new AuthorSelectDialogView();
        try
        {
            var data = Task.Run(() => ((BaseRepository<Author>)myAuthorRepository).GetAllAsync()).Result;
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
    }

    public ObservableCollection<AuthorWrapper> Authors { set; get; } = [];

    public AuthorWrapper CurrentAuthor
    {
        get => GetValue<AuthorWrapper>();
        set => SetValue(value);
    }
}
