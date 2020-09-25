const prompt = require('prompt');
fs = require('fs');

const properties = [
    {
        name: 'superadmin username/email'
    },
    {
        name: 'superadmin password',
        hidden: true
    }
];

console.log("Configure your test enviroment:")

prompt.start();

prompt.get(properties, function (error, result) {
    if (error) { return onError(error); }

var fileContent = `{
    "username": "${result.username}",
    "password": "${result.password}"
}`;

    fs.writeFile('cypress.env.json', fileContent, function (error) {
        if (error) return console.error(error);
        console.log('Configuration saved');
      });
});

function onError(error) {
    console.error(error);
    return true;
}
