const prompt = require('prompt');
const fs = require('fs');

const properties = [
    {
        name: 'username'
    },
    {
        name: 'password',
        hidden: true
    }
];


const configPath = './cypress.env.json'

console.log("Configure your test enviroment")
console.log("Enter CMS superadmin credentials:")

prompt.start();

prompt.get(properties, function (error, result) {
    if (error) { return onError(error); }

var fileContent = `{
    "username": "${result.username}",
    "password": "${result.password}"
}`;

    fs.writeFile(configPath, fileContent, function (error) {
        if (error) return console.error(error);
        console.log('Configuration saved');
    });
});

function onError(error) {
    console.error(error);
    return true;
}
