namespace NewsAggregator.WebAPI.Models.Requests
{
    public class PatchRequestModel
    {
        public List<PatchFieldModel> Fields { get; set; }
    }

    public class PatchFieldModel
    {
        public string Name { get; set; }
        public object? Value { get; set; }
    }
}