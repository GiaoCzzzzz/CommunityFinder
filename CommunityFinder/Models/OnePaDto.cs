using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace CommunityFinder.Models
{
    

    public class OnePaRoot
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("data")] public OnePaData Data { get; set; }
    }

    public class OnePaData
    {
        [JsonPropertyName("results")] public List<OnePaResult> Results { get; set; }
    }

    public class OnePaResult
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("isPrivateClass")] public bool IsPrivateClass { get; set; }
        [JsonPropertyName("userEligibleForClass")] public bool UserEligibleForClass { get; set; }
        [JsonPropertyName("classId")] public string ClassId { get; set; }
        [JsonPropertyName("eventId")] public string EventId { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("outlet")] public string Outlet { get; set; }
        [JsonPropertyName("outletId")] public string OutletId { get; set; }
        [JsonPropertyName("outletUrl")] public string OutletUrl { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("favorite")] public bool Favorite { get; set; }
        [JsonPropertyName("price")] public OnePaPrice Price { get; set; }
        [JsonPropertyName("favoriteID")] public string FavoriteID { get; set; }
        [JsonPropertyName("vacancy")] public int Vacancy { get; set; }
        [JsonPropertyName("maxVacancy")] public int MaxVacancy { get; set; }
        [JsonPropertyName("startDate")] public string StartDateRaw { get; set; } // e.g. "Starts Fri, 03 Oct 2025"
        [JsonPropertyName("sessionTime")] public string SessionTime { get; set; }
        [JsonPropertyName("share")] public OnePaShare Share { get; set; }
        [JsonPropertyName("isSkillsFutureCourse")] public bool IsSkillsFutureCourse { get; set; }
        [JsonPropertyName("organisingCommitteeName")] public string OrganisingCommitteeName { get; set; }
        [JsonPropertyName("aoiL1")] public string AoiL1 { get; set; }
        [JsonPropertyName("aoiL2")] public string AoiL2 { get; set; }
        [JsonPropertyName("aoiL3")] public string AoiL3 { get; set; }
        [JsonPropertyName("productId")] public string ProductId { get; set; }
        [JsonPropertyName("productUrl")] public string ProductUrl { get; set; }
        [JsonPropertyName("minPrice")] public double? MinPriceTop { get; set; }
        [JsonPropertyName("maxPrice")] public double? MaxPriceTop { get; set; }
        [JsonPropertyName("isCategory")] public bool IsCategory { get; set; }
        [JsonPropertyName("allPrices")] public object AllPrices { get; set; }
        [JsonPropertyName("materialFees")] public object MaterialFees { get; set; }
    }

    public class OnePaPrice
    {
        [JsonPropertyName("discount")] public bool Discount { get; set; }
        [JsonPropertyName("publicPrice")] public string PublicPrice { get; set; }   // "$0.00"
        [JsonPropertyName("membersPrice")] public string MembersPrice { get; set; } // "$0.00"
        [JsonPropertyName("minPrice")] public double? MinPrice { get; set; }
        [JsonPropertyName("maxPrice")] public double? MaxPrice { get; set; }
        [JsonPropertyName("discountTitle")] public string DiscountTitle { get; set; }
    }

    public class OnePaShare
    {
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; }
    }

}
