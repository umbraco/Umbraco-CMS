function umbracoCheckUpgrade(result) {
    if (result) {
        if (result.UpgradeType.toLowerCase() != 'none') {
            if (UmbSpeechBubble == null) {
                InitUmbracoSpeechBubble();
            }
            var icon = 'info';
            if (result.UpgradeType.toLowerCase() == 'critical') {
                icon = 'error';
            }

            UmbSpeechBubble.ShowMessage(icon, 'Upgrade Available!', '<a style="text-decoration:none" target="_blank" href="' + result.UpgradeUrl + '">' + result.UpgradeComment + '</a>', true);
        }
    }
}