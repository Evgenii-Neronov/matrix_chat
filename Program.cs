
var matrixService = new MatrixService("http://localhost:8008");

// Аутентификация
var loginSuccess = await matrixService.LoginAsync("neu", "my_pwd123!");

if (!loginSuccess)
{
    Console.WriteLine("Login failed.");
    return;
}

// Отправка сообщения в комнату ROOM-1 (вам нужен ID комнаты)
// var roomId = "!ELCzHCEAEsYDHMABWy:host.docker.internal";
var roomId = await matrixService.CreateRoomAsync("CreatedRoom", "ThisCreatedRoomAlias");

var messageSuccess = await matrixService.SendMessageAsync(roomId, "Hello, ROOM-1!");

if (messageSuccess)
    Console.WriteLine("Message sent successfully.");
else
    Console.WriteLine("Failed to send message.");