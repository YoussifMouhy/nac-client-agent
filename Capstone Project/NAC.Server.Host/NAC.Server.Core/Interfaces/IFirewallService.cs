using System.Threading.Tasks;

namespace NAC.Server.Core.Interfaces
{
	public interface IFirewallService
	{
		void InitializeFirewall();

		/// <param name="ipAddress">The client's IP address (e.g., 192.168.1.55)</param>
		bool AllowClientAccess(string ipAddress);

		/// <param name="ipAddress">The client's IP address</param>
		void RevokeClientAccess(string ipAddress);
	}
}