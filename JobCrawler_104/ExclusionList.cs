namespace JobCrawler_104
{
    public class ExclusionList
    {
        public HashSet<string> CompanyNamesForCSharp { get; init; }
        public HashSet<string> CompanyNamesForGolang { get; init; }
        public HashSet<string> DeferredCompaniesForCSharp { get; init; }
        public HashSet<string> DeferredCompaniesForGolang { get; init; }
        public HashSet<string> SubmittedCompaniesForCSharp { get; init; }
        public HashSet<string> SubmittedCompaniesForGolang { get; init; }
        public HashSet<string> Industries { get; init; }
        public HashSet<string> CompanyKeywords { get; init; }
        public HashSet<string> JobTitleKeywordsForCSharp { get; init; }
        public HashSet<string> JobTitleKeywordsForGolang { get; init; }
    }
}