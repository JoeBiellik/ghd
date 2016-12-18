FROM microsoft/dotnet:runtime

MAINTAINER Joe Biellik <contact@jcbiellik.com>

ARG GHD_VERSION
ENV GHD_DOWNLOAD_URL https://github.com/JoeBiellik/ghd/releases/download/$GHD_VERSION/ghd-$GHD_VERSION-debian.8-x64.tar.gz

WORKDIR /app

RUN curl -SL $GHD_DOWNLOAD_URL | tar -zx --strip=1

ENTRYPOINT ["dotnet", "ghd.dll"]
