const prompt = require('prompt');
const fs = require('fs');

const properties = [
    {
        description: 'Enter your superadmin username/email',
        name: 'username',
        required: true
    },
    {
        description: 'Enter your superadmin password',
        name: 'password',
        hidden: true,
        required: true
    },
    {
        description: 'Enter CMS URL, or leave empty for default(https://localhost:44331)',
        name: 'baseUrl'
    }
];


const configPath = './.env'

console.log("Configure your test enviroment")

prompt.start();

prompt.get(properties, function (error, result) {
    if (error) { return onError(error); }

var fileContent = `UMBRACO_USER_LOGIN=${result.username}
UMBRACO_USER_PASSWORD=${result.password}
URL=${result.baseUrl || "https://localhost:44331"}`;

    fs.writeFile(configPath, fileContent, function (error) {
        if (error) return console.error(error);
        console.log('Configuration saved');
    });
});

function onError(error) {
    console.error(error);
    return true;
}
