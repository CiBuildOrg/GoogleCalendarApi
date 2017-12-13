namespace Mvc.Server.DataObjects.Response
{
    public class ServiceResponse<T> where T: class, new ()
    {
        public T Result { get; set; }
        public bool Success { get; set; }

        public static ServiceResponse<T> Fail
        {
            get { return new ServiceResponse<T> { Result = new T(), Success = false }; }
        }
    }
}
