using ASP.Services.Kdf;

namespace ASP.Data.DAL
{
	public class DataAccessor
	{
		private readonly DataContext _dataContext;
		private readonly IKdfService _kdfService;

		public UserDao UserDao { get; private set; }
		public DataAccessor(DataContext dataContext, IKdfService kdfService)
		{
			_dataContext = dataContext;
			_kdfService = kdfService;
			UserDao = new UserDao(dataContext, kdfService);
		}
	}
}
