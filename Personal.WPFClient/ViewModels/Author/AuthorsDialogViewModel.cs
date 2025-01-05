using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.Author;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class AuthorsDialogViewModel : ViewModelDialogBase
{
    private readonly IAuthorRepository myAuthorRepository;

    public AuthorsDialogViewModel(IAuthorRepository authorRepository, ILayoutRepository layoutRepository)
    {
        myAuthorRepository = authorRepository;
        myLayoutRepository = layoutRepository;
        Properties.Id = MenuAndDocumentIds.AuthorSelectMenuId;
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
