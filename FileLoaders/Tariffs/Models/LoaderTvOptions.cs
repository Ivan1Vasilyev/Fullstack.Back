namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderTvOptions(int? Channels, int? HdChannels, int? UhdChannels, int? InteractiveChannels, LoaderDevice? TvPristavka, LoaderDevice? TvPristavka2 = null) : ILoaderCode
    {
        string _codeCache;

        string GetCode()
        {
            if (_codeCache == null)
            {
                _codeCache = $"tv";

                if (Channels.HasValue)
                {
                    _codeCache += $"-channels-{Channels}";
                }

                if (HdChannels.HasValue)
                {
                    _codeCache += $"-hd-{HdChannels}";
                }

                if (UhdChannels.HasValue)
                {
                    _codeCache += $"-uhd-{UhdChannels}";
                }

                if (InteractiveChannels.HasValue)
                {
                    _codeCache += $"-interactive-{InteractiveChannels}";
                }

                if(TvPristavka != null && TvPristavka.InComplect)
                {
                    _codeCache += $"-{TvPristavka.Code}";
                }

                if (TvPristavka2 != null)
                {
                    _codeCache += $"-{TvPristavka2.Code}";
                }
            }

            return _codeCache;
        }

        public string Code => GetCode();

        public LoaderTvOptions NullIfEmpty()
        {
            if (Channels.HasValue || HdChannels.HasValue || UhdChannels.HasValue || InteractiveChannels.HasValue || TvPristavka is not null || TvPristavka2 is not null)
            {
                return this;
            }
            return null;
        }
    };
}
