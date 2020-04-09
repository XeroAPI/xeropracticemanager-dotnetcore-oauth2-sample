using System.Collections.Generic;
using System.Xml.Serialization;

namespace XeroPracticeManagerOAuth2Sample.Models.ClientApi
{
	[XmlRoot(ElementName = "AccountManager")]
	public class AccountManager
	{
		[XmlElement(ElementName = "UUID")]
		public string UUID { get; set; }
		[XmlElement(ElementName = "Name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName = "JobManager")]
	public class JobManager
	{
		[XmlElement(ElementName = "UUID")]
		public string UUID { get; set; }
		[XmlElement(ElementName = "Name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName = "Client")]
	public class Client
	{
		[XmlElement(ElementName = "UUID")]
		public string UUID { get; set; }
		[XmlElement(ElementName = "Name")]
		public string Name { get; set; }
		[XmlElement(ElementName = "Address")]
		public string Address { get; set; }
		[XmlElement(ElementName = "City")]
		public string City { get; set; }
		[XmlElement(ElementName = "Region")]
		public string Region { get; set; }
		[XmlElement(ElementName = "PostCode")]
		public string PostCode { get; set; }
		[XmlElement(ElementName = "Country")]
		public string Country { get; set; }
		[XmlElement(ElementName = "PostalAddress")]
		public string PostalAddress { get; set; }
		[XmlElement(ElementName = "PostalCity")]
		public string PostalCity { get; set; }
		[XmlElement(ElementName = "PostalRegion")]
		public string PostalRegion { get; set; }
		[XmlElement(ElementName = "PostalPostCode")]
		public string PostalPostCode { get; set; }
		[XmlElement(ElementName = "PostalCountry")]
		public string PostalCountry { get; set; }
		[XmlElement(ElementName = "Phone")]
		public string Phone { get; set; }
		[XmlElement(ElementName = "Fax")]
		public string Fax { get; set; }
		[XmlElement(ElementName = "Website")]
		public string Website { get; set; }
		[XmlElement(ElementName = "ReferralSource")]
		public string ReferralSource { get; set; }
		[XmlElement(ElementName = "ExportCode")]
		public string ExportCode { get; set; }
		[XmlElement(ElementName = "IsProspect")]
		public string IsProspect { get; set; }
		[XmlElement(ElementName = "IsArchived")]
		public string IsArchived { get; set; }
		[XmlElement(ElementName = "IsDeleted")]
		public string IsDeleted { get; set; }
		[XmlElement(ElementName = "AccountManager")]
		public AccountManager AccountManager { get; set; }
		[XmlElement(ElementName = "JobManager")]
		public JobManager JobManager { get; set; }
		[XmlElement(ElementName = "Contacts")]
		public string Contacts { get; set; }
	}

	[XmlRoot(ElementName = "Clients")]
	public class ClientList
	{
		[XmlElement(ElementName = "Client")]
		public List<Client> Clients { get; set; }
	}

	[XmlRoot(ElementName = "Response")]
	public class ClientListResponse
	{
		[XmlElement(ElementName = "Status")]
		public string Status { get; set; }
		[XmlElement(ElementName = "Clients")]
		public ClientList ClientList { get; set; }
		[XmlAttribute(AttributeName = "api-method")]
		public string ApiMethod { get; set; }
	}
}
