using AcademicSocialNetwork.Models;

namespace AcademicSocialNetwork.ViewModels;

public class ConnectionsViewModel
{
    public List<Connection> Accepted  { get; set; } = new();
    public List<Connection> Incoming  { get; set; } = new(); // pending, sent TO current user
    public List<Connection> Outgoing  { get; set; } = new(); // pending, sent BY current user
    public int CurrentUserId          { get; set; }
}