FROM microsoft/aspnetcore:2.0-stretch AS base
WORKDIR /app

FROM microsoft/aspnetcore-build:2.0-stretch AS build
WORKDIR /src
COPY ["quickstartcore.csproj", ""]
RUN dotnet restore "quickstartcore.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "quickstartcore.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "quickstartcore.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

COPY init_container.sh /app

ENV SSH_PASSWD "root:Docker!"
RUN apt-get update \
	  && apt-get install -y --no-install-recommends net-tools nano openssh-server \
	  && echo "$SSH_PASSWD" | chpasswd \
	  && chmod 755 /app/init_container.sh

COPY sshd_config /etc/ssh/

EXPOSE 80 2222

ENTRYPOINT ["/app/init_container.sh"]
