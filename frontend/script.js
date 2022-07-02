function SafeFormat(text) {
    var d = document.createElement("div")
    d.innerText = text
    return d.innerHTML
}

function tfetch(url, method = "GET", body = "") {
    return ifetch(url, false, method, body)
}

function jfetch(url, method = "GET", body = "") {
    return ifetch(url, true, method, body)
}

function ifetch(url, asjson = true, method = "GET", body = "") {
    return new Promise((resolve, reject) => {
        if(method == "GET" || method == "HEAD") {
            fetch(url, {
                method: method,
                headers: {
                    "token": localStorage.token
                }
            }).then(res => {
                res.text().then(res => {
                    if(asjson) {
                        try {
                            resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        resolve(res)
                    }
                })
            })
        } else {
            fetch(url, {
                method: method,
                body: body,
                headers: {
                    "token": localStorage.token
                }
            }).then(res => {
                res.text().then(res => {
                    if(asjson) {
                        try {
                            resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        resolve(res)
                    }
                })
            })
        }
    })
}