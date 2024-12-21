using System;

namespace WPFClient.Configuration;

public static class MenuAndDocumentIds
{
    public static readonly Guid AuthorMenuId = Guid.Parse("{B9C94D32-4329-4461-BC35-45FA107A2623}");
    public static readonly Guid BookMenuId = Guid.Parse("{878D9D8C-279A-419E-96C7-68991E33D5DF}");
    public static readonly Guid ReadPagingMenuId = Guid.Parse("{C8930C35-5C7F-4664-BD6A-2E52E4CF94CA}");

    public static readonly Guid BookPartitionMenuId = Guid.Parse("{C8930C35-5C7F-4664-BD6A-2E52E4CF94CA}");
    public static readonly Guid GenreMenuId = Guid.Parse("{C8930C35-5C7F-4664-BD6A-2E52E4CF94CA}");



    public static readonly Guid BookDocumentId = Guid.Parse("{20754334-8109-4ED9-8FCA-5BA3FDE504A3}");
    public static readonly Guid AuthorDocumentId = Guid.Parse("{CCFB0A1C-461B-4B8F-8453-26000EED0C13}");
}
