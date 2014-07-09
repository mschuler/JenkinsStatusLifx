namespace JenkinsStatusLifx.ViewModels
{
    public class AllBulbAdapter : BulbAdapterBase
    {
        public AllBulbAdapter()
        {
            // TODO: i18n
            Name = "All Lights";
        }

        public override bool IsBulbGroup { get { return true; } }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}