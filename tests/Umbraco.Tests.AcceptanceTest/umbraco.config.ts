const umbracoConfig = {
    environment: {
      baseUrl: process.env.URL || "http://localhost:8080"
    },
    user: {
      login: process.env.UMBRACO_USER_LOGIN || "nge@umbraco.dk",
      password: process.env.UMBRACO_USER_PASSWORD || "1234567890"
    },
    member: {
      login: process.env.UMBRACO_MEMBER_LOGIN || "member@example.com",
      password: process.env.UMBRACO_MEMBER_PASSWORD || "Umbraco9Rocks!"
    }
  };
  
  export { umbracoConfig };