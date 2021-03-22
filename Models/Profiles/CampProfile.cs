﻿using AutoMapper;
using CoreCodeCamp.Data;

namespace AspRestApiWorkshop.Models.Profiles
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            CreateMap<Camp, CampModel>()
                .ForMember(campModel => campModel.Venue, 
                memberOptions => memberOptions.MapFrom(camp => camp.Location.VenueName))
                .ReverseMap();

            CreateMap<Camp, CampFriendlyModel>()
                .ForMember(campModel => campModel.NameWithMoniker,
                memberOptions => memberOptions.MapFrom(camp => $"{camp.Name} ({camp.Moniker})"))
                .ForMember(campModel => campModel.Venue,
                memberOptions => memberOptions.MapFrom(camp => camp.Location.VenueName))
                .ReverseMap();

            CreateMap<Camp, CampModelForSimpleCreation>().ReverseMap();

            CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(talk => talk.Camp, opt => opt.Ignore())
                .ForMember(talk => talk.Speaker, opt => opt.Ignore());

            CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();
        }
    }
}