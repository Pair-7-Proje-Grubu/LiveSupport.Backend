namespace ChatAppServer.WebAPI.Models
{
    public sealed class User
    {

        public User() 
        {
            Id = Guid.NewGuid();
            Status = "offline";  // Register dan sonra varsayılan değeri "offline" olarak ayarlıyoruz
            Role = 0; // Varsayılan olarak normal kullanıcı
        }
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   //yeni ekleme
        public string Status { get; set; } = string.Empty;
        public int Role { get; set; }  // 0: Normal kullanıcı, 1: Moderatör

        //public string Avatar { get; set; } = string.Empty;


    }
}
