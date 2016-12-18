FROM microsoft/dotnet:runtime

MAINTAINER Joe Biellik <contact@jcbiellik.com>

ARG BUILD_DATE
ARG VCS_REF
ARG VERSION
ENV GHD_DOWNLOAD_URL https://github.com/JoeBiellik/ghd/releases/download/$VERSION/ghd-$VERSION-debian.8-x64.tar.gz

LABEL org.label-schema.build-date=$BUILD_DATE \
    org.label-schema.name="ghd" \
    org.label-schema.description="Simple cross platform service to run a command when triggered by a GitHub webhook." \
    org.label-schema.url="https://github.com/JoeBiellik/ghd" \
    org.label-schema.vcs-ref=$VCS_REF \
    org.label-schema.vcs-url="https://github.com/JoeBiellik/ghd" \
    org.label-schema.vendor="Joe Biellik" \
    org.label-schema.version=$VERSION \
    org.label-schema.schema-version="1.0"

WORKDIR /app

RUN curl -SL $GHD_DOWNLOAD_URL | tar -zx --strip=1

ENTRYPOINT ["dotnet", "ghd.dll"]
