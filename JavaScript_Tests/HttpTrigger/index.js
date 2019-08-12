module.exports = async function (context, req, TokenBindingOutput) {
  // Note: console is local debug, context is azure portal debug
  var fs = require('file-system');
  var fetch = require('isomorphic-fetch');
  var Dropbox = require('dropbox').Dropbox;

  TokenBindingOutput = "m1fQQ80HewAAAAAAAAAALSLpcJW5-u3y7xCVh5CDJzz85NWxWFxgK7aAOVS1Qp5k";
  context.res = {
    status: 400,
    body: "Check your dropbox account to see a new document listing all your files."
    };
    
  dbx = new Dropbox({
    fetch: fetch,
    accessToken: TokenBindingOutput  
    })

  .filesListFolder({path: ''}) // returns a promise for Dropboxtypes.files
  .then(response => { 
    var datastring = "Files: ";
    for (let i = 0; i < response.entries.length; i++){
      console.log(response.entries[i].name);
      context.log(response.entries[i].name); 
      datastring += response.entries[i].name + ", ";
    }
      dbx = new Dropbox({
        fetch: fetch,
        accessToken: TokenBindingOutput
        })
    
      .filesUpload({path: "/listdropboxfiles", contents: datastring}).then(function(response){
        console.log("Check your dropbox account to see a new document listing all your files.");
        context.log("Check your dropbox account to see a new document listing all your files.");
        console.log(response);        
      })
      .catch(function(error){
        console.error(error);
        context.log(error);
      });
  })
  .catch(function(error){
    console.error(error);
    context.log("An error occurred in outer catch statement");
    context.log(error);
  });
    
}