//#region src/bundle/lang/da.ts
var e = { search: {
	treeHeader: "Søgning",
	tableColumnAlias: "Alias",
	tableColumnHealthStatus: "Status",
	tableColumnDocumentCount: "Antal dokumenter",
	healthStatus: (e) => {
		switch (e) {
			case "Empty": return "Tom";
			case "Corrupted": return "Fejl: Korrupt";
			case "Rebuilding": return "Gen-indekserer";
			case "Healthy": return "God";
			default: return "Fejl: Ukendt";
		}
	},
	documentCount: (e) => {
		switch (e) {
			case 0: return "Tom";
			case 1: return "1 dokument";
			default: return `${e} dokumenter`;
		}
	},
	collectionActionReload: "Opdater liste",
	entityActionRebuildIndex: "Genopbyg indeks",
	rebuildConfirmHeadline: "Genopbyg søgeindeks",
	rebuildConfirmMessage: "Er du sikker på, at du vil genopbygge søgeindekset? Denne handling kan tage et stykke tid afhængigt af størrelsen på dit indhold.",
	rebuildConfirmLabel: "Genopbyg indeks",
	rebuildStartedMessage: "Genopbygningen af søgeindeks \"{0}\" er startet. Du kan fortsætte med at arbejde, mens processen kører i baggrunden.",
	rebuildCompletedTitle: "Genopbygning af søgeindeks fuldført",
	rebuildCompletedMessage: "Genopbygningen af søgeindeks \"{0}\" er blevet fuldført.",
	rebuildingIndex: "Genopbygger indeks...",
	rebuildIndex: "Genopbyg indeks",
	indexInfo: "Indeks information",
	indexAlias: "Indeks alias",
	providerName: "Udbyderens navn",
	searchBox: "Søg",
	searchPlaceholder: "Søg i indeks...",
	searchButton: "Søg",
	noResults: "Ingen resultater fundet",
	resultsCount: (e) => `Fandt ${e} resultat${e === 1 ? "" : "er"}`,
	tableColumnName: "Navn",
	tableColumnEntityType: "Objekttype",
	statsBoxLabel: "Statistik",
	searchBoxLabel: "Søgning",
	searching: "Søger...",
	searchFailed: "Søgning fejlede",
	searchComplete: (e) => `Søgning færdig. Fandt ${e} resultat${e === 1 ? "" : "er"}`,
	openEntity: (e, t) => `Åbn ${e} med ID ${t}`,
	searchFormLabel: (e) => `Søg i ${e} indeks`,
	searchInputLabel: "Søgeforespørgsel",
	searchInputAriaLabel: (e) => `Indtast søgeforespørgsel for ${e} indeks`,
	searchButtonAriaLabel: "Udfør søgning",
	searchHint: "Tryk Enter eller klik på Søg-knappen for at udføre søgningen",
	loading: "Indlæser søgeresultater",
	resultsRegion: "Søgeresultater",
	resultsTable: "Tabel med søgeresultater",
	paginationLabel: "Sider med søgeresultater",
	cultureSelectLabel: "Kultur",
	searchDisabled: "Søgning er deaktiveret, fordi indekset ikke er sundt. Nuværende status:",
	searchError: "Der opstod en fejl under søgningen. Prøv venligst igen."
} };
//#endregion
export { e as default };

//# sourceMappingURL=da-BNWAqFww.js.map