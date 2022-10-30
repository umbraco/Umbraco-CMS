export declare class Sa11y {
	containerIgnore: string;
	contrastIgnore: string;
	readabilityIgnore: string;
	headerIgnore: string;
	outlineIgnore: string;
	imageIgnore: string;
	linkIgnore: string;
	store: {
		getItem: (key: string) => string;
		setItem: (key: string, value: string) => boolean;
	}
	panelActive: boolean;
	errorCount: number;
	warningCount: number;
	root: HTMLElement;
	panel: HTMLElement;

	contrast: Array<HTMLElement>;
	images: Array<HTMLImageElement>;
	headings: Array<HTMLHeadingElement>;
	headingOne: Array<HTMLHeadingElement>;
	links: Array<HTMLAnchorElement>;
	readability: Array<HTMLElement>;

	language: string | null;
	paragraphs: Array<HTMLParagraphElement>;
	lists: Array<HTMLLIElement>;
	spans: Array<HTMLSpanElement>;
	blockquotes: Array<HTMLQuoteElement>;
	tables: Array<HTMLTableElement>;
	pdf: Array<HTMLAnchorElement>;
	strongitalics: Array<HTMLElement>;
	inputs: Array<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>;
	customErrorLinks: Array<HTMLElement>;

	iframes: Array<HTMLIFrameElement | HTMLAudioElement | HTMLVideoElement>;
	videos: Array<HTMLIFrameElement | HTMLAudioElement | HTMLVideoElement>;
	audio: Array<HTMLIFrameElement | HTMLAudioElement | HTMLVideoElement>;
	datavisualizations: Array<HTMLIFrameElement | HTMLAudioElement | HTMLVideoElement>;
	embeddedContent: Array<HTMLIFrameElement | HTMLAudioElement | HTMLVideoElement>;

	constructor(options?: {
		checkRoot?: string;
		containerIgnore?: string;
		contrastIgnore?: string;
		outlineIgnore?: string;
		headerIgnore?: string;
		imageIgnore?: string;
		linkIgnore?: string;
		linkIgnoreSpan?: string;
		linksToFlag?: string;
		nonConsecutiveHeadingIsError?: boolean;
		flagLongHeadings?: boolean;
		showGoodLinkButton?: boolean;
		detectSPArouting?: boolean;
		doNotRun?: string;

		// Readability
		readabilityPlugin?: boolean;
		readabilityRoot?: string;
		readabilityLang?: 'en' | 'fr' | 'es' | 'de' | 'nl' | 'it' | 'sv' | 'fi' | 'da' | 'no' | 'nb' | 'nn';
		readabilityIgnore?: string;

		// Other plugins
		contrastPlugin?: boolean;
		formLabelsPlugin?: boolean;
		linksAdvancedPlugin?: boolean;
		customChecks?: Sa11yCustomChecks;

		// QA rulesets
		badLinksQA?: boolean;
		strongItalicsQA?: boolean;
		pdfQA?: boolean;
		langQA?: boolean;
		blockquotesQA?: boolean;
		tablesQA?: boolean;
		allCapsQA?: boolean;
		fakeHeadingsQA?: boolean;
		fakeListQA?: boolean;
		duplicateIdQA?: boolean;
		underlinedTextQA?: boolean;
		pageTitleQA?: boolean;
		subscriptQA?: boolean;

		// Embedded content rulesets
		embeddedContentAll?: boolean;
		embeddedContentAudio?: boolean;
		embeddedContentVideo?: boolean;
		embeddedContentDataViz?: boolean;
		embeddedContentTitles?: boolean;
		embeddedContentGeneral?: boolean;

		// Embedded content
		videoContent?: string;
		audioContent?: string;
		dataVizContent?: string;
		embeddedContent?: string;
	})

	initialize: () => void;
	buildSa11yUI: () => void;
	globals: () => void;
	mainToggle: () => void;
	utilities: () => void;
	isElementHidden: ($el: HTMLElement) => boolean;
	escapeHTML: (text: string) => string;
	sanitizeForHTML: (string: string) => string;
	computeTextNodeWithImage: ($el: HTMLElement) => string;
	debounce: (callback: () => unknown, wait: number) => () => unknown;
	fnIgnore: (element: HTMLElement, selector: string) => HTMLElement;
	computeAriaLabel: ($el: HTMLElement) => string;
	findVisibleParent: (element: HTMLElement, property: string, value: string) => HTMLElement | null;
	offsetTop: ($el: HTMLElement) => {
		top: number
	};
	addPulse: ($el: HTMLElement) => HTMLElement;
	createAlert: (alertMessage: HTMLElement, errorPreview: HTMLElement) => HTMLElement | HTMLElement;
	removeAlert: () => void;
	getText: ($el: HTMLElement) => string;
	getNextSibling: (elem: HTMLElement, selector: string) => HTMLElement;
	settingPanelToggles: () => void;
	skipToIssueTooltip: () => void;
	detectPageChanges: () => void;
	checkAll: () => Promise<void>;
	resetAll: (restartPanel?: boolean) => void;
	initializeTooltips: () => void;
	detectOverflow: () => void;
	nudge: () => void;
	updateBadge: () => void;
	updatePanel: () => void;
	buildPanel: () => void;
	skipToIssue: () => void;
	findElements: () => void;
	find: (selectors: string, exclude: string, rootTypealert: string) => Array;
	annotate: (type: 'Error' | 'Warning' | 'Good', content: string, inline?: boolean) => string;
	annotateBanner: (type: 'Error' | 'Warning' | 'Good', content: string) => string;
	checkHeaders: () => void;
	checkLinkText: () => void
	checkLinksAdvanced: () => void;
	checkAltText: () => void;
	containsAltTextStopWords: (alt: string) => Array<string | null>;
	checkLabels: () => void;
	checkEmbeddedContent: () => void;
	checkQA: () => void;
	checkContrast: () => void;
	checkReadability: () => void;
}

export declare const Lang: {
	langStrings: Sa11yLang['strings'];
	addI18n(strings: Sa11yLang['strings']): void;
	_(string: string): string;
	sprintf(string: string, ...args: any[]): string;
	translate(string: string): string;
};

export declare const LangEn: Sa11yLang;

export declare const LangFr: Sa11yLang;

export declare const LangPl: Sa11yLang;

export declare const LangUa: Sa11yLang;

export declare const LangSv: Sa11yLang;

export declare type Sa11yLang = {
	strings: {
		LANG_CODE: string;
		MAIN_TOGGLE_LABEL: string;
		CONTAINER_LABEL: string;
		ERROR: string;
		ERRORS: string;
		WARNING: string;
		WARNINGS: string;
		GOOD: string;
		ON: string;
		OFF: string;
		ALERT_TEXT: string;
		ALERT_CLOSE: string;
		SHOW_OUTLINE: string;
		HIDE_OUTLINE: string;
		SHOW_SETTINGS: string;
		HIDE_SETTINGS: string;
		PAGE_OUTLINE: string;
		SETTINGS: string;
		CONTRAST: string;
		FORM_LABELS: string;
		LINKS_ADVANCED: string;
		DARK_MODE: string;
		SHORTCUT_SCREEN_READER: string;
		SHORTCUT_TOOLTIP: string;
		NEW_TAB: string;
		PANEL_HEADING: string;
		PANEL_STATUS_NONE: string;
		PANEL_ICON_WARNINGS: string;
		PANEL_ICON_TOTAL: string;
		NOT_VISIBLE_ALERT: string;
		ERROR_MISSING_ROOT_TARGET: string;
		HEADING_NOT_VISIBLE_ALERT: string;

		// Alternative text module stop words
		SUSPICIOUS_ALT_STOPWORDS: Array<string>;
		PLACEHOLDER_ALT_STOPWORDS: Array<string>;
		PARTIAL_ALT_STOPWORDS: Array<string > ,
		WARNING_ALT_STOPWORDS: Array<string>;
		NEW_WINDOW_PHRASES: Array<string>;

		// Only some items in list would need to be translated.
		FILE_TYPE_PHRASES: Array<string>;

		// Readability
		LANG_READABILITY: string;
		LANG_AVG_SENTENCE: string;
		LANG_COMPLEX_WORDS: string;
		LANG_TOTAL_WORDS: string;
		LANG_VERY_DIFFICULT: string;
		LANG_DIFFICULT: string;
		LANG_FAIRLY_DIFFICULT: string;
		LANG_GOOD: string;
		READABILITY_NO_P_OR_LI_MESSAGE: string;
		READABILITY_NOT_ENOUGH_CONTENT_MESSAGE: string;

		// Headings
		HEADING_NON_CONSECUTIVE_LEVEL: string;
		HEADING_EMPTY: string;
		HEADING_LONG: string;
		HEADING_FIRST: string;
		HEADING_MISSING_ONE: string;
		HEADING_EMPTY_WITH_IMAGE: string;
		PANEL_HEADING_MISSING_ONE: string;

		// Links
		LINK_EMPTY: string;
		LINK_EMPTY_LINK_NO_LABEL: string;
		LINK_LABEL: string;
		LINK_STOPWORD: string;
		LINK_BEST_PRACTICES: string;
		LINK_URL: string;

		// Links advanced
		NEW_TAB_WARNING: string;
		FILE_TYPE_WARNING: string;
		LINK_IDENTICAL_NAME: string;

		// Images
		MISSING_ALT_LINK_BUT_HAS_TEXT_MESSAGE: string;
		MISSING_ALT_LINK_MESSAGE: string;
		MISSING_ALT_MESSAGE: string;
		LINK_IMAGE_BAD_ALT_MESSAGE: string;
		LINK_IMAGE_PLACEHOLDER_ALT_MESSAGE: string;
		LINK_IMAGE_SUS_ALT_MESSAGE: string;
		LINK_ALT_HAS_BAD_WORD_MESSAGE: string;
		ALT_PLACEHOLDER_MESSAGE: string;
		ALT_HAS_SUS_WORD: string;
		LINK_IMAGE_ARIA_HIDDEN: string;
		LINK_IMAGE_NO_ALT_TEXT: string;
		LINK_IMAGE_HAS_TEXT: string;
		LINK_IMAGE_LONG_ALT: string;
		LINK_IMAGE_ALT_WARNING: string;
		LINK_IMAGE_ALT_AND_TEXT_WARNING: string;
		IMAGE_FIGURE_DECORATIVE: string;
		IMAGE_FIGURE_DUPLICATE_ALT: string;
		IMAGE_DECORATIVE: string;
		IMAGE_ALT_TOO_LONG: string;
		IMAGE_PASS: string;

		// Labels
		LABELS_MISSING_IMAGE_INPUT_MESSAGE: string;
		LABELS_INPUT_RESET_MESSAGE: string;
		LABELS_ARIA_LABEL_INPUT_MESSAGE: string;
		LABELS_NO_FOR_ATTRIBUTE_MESSAGE: string;
		LABELS_MISSING_LABEL_MESSAGE: string;

		// Embedded content
		EMBED_VIDEO: string;
		EMBED_AUDIO: string;
		EMBED_DATA_VIZ: string;
		EMBED_MISSING_TITLE: string;
		EMBED_GENERAL_WARNING: string;

		// Quality assurance
		QA_BAD_LINK: string;
		QA_BAD_ITALICS: string;
		QA_PDF: string;
		QA_PAGE_LANGUAGE: string;
		QA_PAGE_TITLE: string;
		QA_BLOCKQUOTE_MESSAGE: string;
		QA_FAKE_HEADING: string;
		QA_SHOULD_BE_LIST: string;
		QA_UPPERCASE_WARNING: string;
		QA_DUPLICATE_ID: string;
		QA_TEXT_UNDERLINE_WARNING: string;
		QA_SUBSCRIPT_WARNING: string;

		// Tables
		TABLES_MISSING_HEADINGS: string;
		TABLES_SEMANTIC_HEADING: string;
		TABLES_EMPTY_HEADING: string;

		// Contrast
		CONTRAST_ERROR: string;
		CONTRAST_WARNING: string;
		CONTRAST_INPUT_ERROR: string;
	},
};

export declare class Sa11yCustomChecks {
	sa11y: Sa11y;

	setSa11y(sa11y: Sa11y): void;

	check(): void;
}
