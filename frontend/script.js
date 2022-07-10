const params = new URLSearchParams(window.location.search)


function FormatGroup(group, showEditButton = false) {
    return ` <div class="card">
        <h3 class="nomargintopbottom">${SafeFormat(group.name)}</h3>
        <div class="nomargintopmarginleft">${group.description}</div>
        ${showEditButton ? `<input onclick="EditGroup('${group.groupId}')" type="button" value="Edit">`: ``}
    </div>`
}

/*
Unpublished = 0,
Pending = 1,
Approved = 2,
Declined = 3
*/
function UpdateModStatus(newStatus, modId) {
    tfetch(`/api/v1/updatemodstatus/${modId}`, "POST", newStatus).then(res => {
        TextBoxGood("text", res)
    }).catch(res => {
        TextBoxError("text", res)
    })
}

function FormatFile(mod, file, showEditButton = false) {
    return ` <div class="card">
        <h3 class="nomargintopbottom">${SafeFormat(file.filename)}</h3>
        <div class="nomargintopmarginleft">${file.sHA256}</div>
        <div class="nomargintopmarginleft">${file.sizeString}</div>
        ${GetPreview(mod.uploadedModId, file)}
        ${showEditButton ? `<input onclick="RemoveFile('${file.sHA256}')" type="button" value="Delete" class="red">`: ``}
        <input onclick="DownloadFile('${mod.uploadedModId}', '${file.sHA256}')" type="button" value="Download">
        ${file.supportsModInfoPopulation && showEditButton ? `<input onclick="PopulateModInfo('${file.sHA256}')" type="button" value="Populate mod info">` : ""}
    </div>`
}

function GetPreview(modId, modFile) {
    var ext = modFile.filename.toLowerCase();
    var url = `/cdn/${modId}/${modFile.sHA256}`
    if(ext.endsWith(".png") || ext.endsWith(".jpg") || ext.endsWith(".jpeg") || ext.endsWith(".gif")) {
        return `<img src="${url}"><br>`
    }
    if(ext.endsWith(".ogg") ||ext.endsWith(".mp3") || ext.endsWith(".wav")) {
        return `<audio src="${url}" controls></audio><br>`
    }
    if(ext.endsWith(".mp4") ||ext.endsWith(".webm") || ext.endsWith(".mkv")) {
        return `<video src="${url}" controls></video><br>`
    }
    return ``
}

function GetModStatusFromInt(s) {
    switch(s) {
        case 0: return "Unpublished"
        case 1: return "Pending"
        case 2: return "Approved"
        case 3: return "Declined"
    }
}

function FormatMod(mod, showEditButton = false, otherButtons = true) {
    var preview = ``
    for(let i = 0; i < mod.files.length; i++) {
        preview = GetPreview(mod.uploadedModId, mod.files[i])
        if(preview) break;
    }
    return `<div class="card">
                <h3 class="nomargintopbottom">${SafeFormat(mod.name)}</h3>
                ${showEditButton ? `<b>${GetModStatusFromInt(mod.status)}</b>` : ``}
                ${mod.version ? `<div class="nomargintopmarginleft">V. ${SafeFormat(mod.version)}</div>` : ``}
                ${mod.groupVersion ? `<div class="nomargintopmarginleft">for version ${SafeFormat(mod.groupVersion)}</div>` : ``}
                <div class="nomargintopmarginleft">by ${SafeFormat(GetPackageName(mod.author))}</div>
                ${mod.author != mod.uploader ? `<div class="nomargintopmarginleft" style="font-size: .8em;">uploaded by ${SafeFormat(mod.uploader)}</div>` : ``}
                <div class="margintopmarginleft">${SafeFormat(mod.description)}</div>
                ${preview}
                ${otherButtons ? `<input onclick="Download('${mod.uploadedModId}')" type="button" value="Download">
                <input onclick="Details('${mod.uploadedModId}')" type="button" value="Details">` : ``}
                ${showEditButton ? `<input onclick="Edit('${mod.uploadedModId}')" type="button" value="Edit">` : ``}
            </div>`
}

function EditGroup(groupId) {
    location = `/editgroup?groupid=${groupId}`
}

function DeleteGroup(groupId) {
    location = `/editgroup?groupid=${groupId}`
}

function DownloadFile(modId, fileId) {
    window.open(`/cdn/${modId}/${fileId}`, "_blank")
}

function GetMod(modId) {
    for(let i = 0; i < mods.length; i++) {
        if(mods[i].uploadedModId == modId) return mods[i]
    }
}

function Download(modId) {
    var mod = GetMod(modId)
    mod.files.forEach(x => {
        DownloadFile(mod.uploadedModId, x.sHA256)
    })
}

function Details(modId) {
    location = `/mod/${modId}`
}

function Edit(modId) {
    location = `/upload?modid=${modId}`
}

function GetPackageName(x) {
    return x
}

function TextBoxError(id, text) {
    ChangeTextBoxProperty(id, "#EE0000", text)
}

function TextBoxText(id, text) {
    ChangeTextBoxProperty(id, "#03cffc", text)
}

function TextBoxGood(id, text) {
    ChangeTextBoxProperty(id, "#00EE00", text)
}

function HideTextBox(id) {
    document.getElementById(id).style.visibility = "hidden"
}

function ChangeTextBoxProperty(id, color, innerHtml) {
    var text = document.getElementById(id)
    text.style.visibility = "visible"
    text.style.border = color + " 1px solid"
    text.innerHTML = innerHtml
}

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
            }).then(r => {
                r.text().then(res => {
                    if(asjson) {
                        try {
                            if(r.status != 200) reject(JSON.parse(res))
                            else resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        if(r.status != 200) reject(res)
                        else resolve(res)
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
            }).then(r => {
                
                r.text().then(res => {
                    if(asjson) {
                        try {
                            if(r.status != 200) reject(JSON.parse(res))
                            else resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        if(r.status != 200) reject(res)
                        else resolve(res)
                    }
                })
            })
        }
    })
}