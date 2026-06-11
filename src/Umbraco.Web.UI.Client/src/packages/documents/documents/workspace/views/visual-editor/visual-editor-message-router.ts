/**
 * Map of guest-script message types to their handler signatures.
 * Mirrors the messages sent by `src/apps/visual-editor/injected.ts`.
 */
export type UmbVisualEditorGuestMessageHandlers = {
	'umb:ve:property-selected': (data: { propertyAlias: string }) => void;
	'umb:ve:block-selected': (data: { blockKey: string; contentTypeAlias: string }) => void;
	'umb:ve:block-add': (data: { siblingBlockKey: string; insertIndex?: number }) => void;
	'umb:ve:block-add-to-property': (data: { propertyAlias: string; insertIndex?: number }) => void;
	'umb:ve:block-add-to-area': (data: { parentBlockKey: string; areaAlias: string; insertIndex?: number }) => void;
	'umb:ve:block-move': (data: {
		blockKey: string;
		targetIndex?: number;
		targetParentBlockKey?: string;
		targetAreaAlias?: string;
	}) => void;
	'umb:ve:block-delete': (data: { blockKey: string }) => void;
	'umb:ve:block-reorder': (data: { blockKey: string; toIndex?: number }) => void;
	'umb:ve:region-map': (data: { regions?: Array<unknown> }) => void;
};

/**
 * Routes postMessage events from the visual editor guest script to typed handlers.
 * Rejects messages that do not originate from the preview iframe (origin + source checked).
 */
export class UmbVisualEditorMessageRouter {
	#handlers: Partial<UmbVisualEditorGuestMessageHandlers>;
	#getExpectedOrigin: () => string | undefined;
	#getExpectedSource: () => Window | null | undefined;

	constructor(args: {
		handlers: Partial<UmbVisualEditorGuestMessageHandlers>;
		getExpectedOrigin: () => string | undefined;
		getExpectedSource: () => Window | null | undefined;
	}) {
		this.#handlers = args.handlers;
		this.#getExpectedOrigin = args.getExpectedOrigin;
		this.#getExpectedSource = args.getExpectedSource;
	}

	readonly onMessage = (event: MessageEvent) => {
		const data = event.data;
		if (!data || data.source !== 'umb-visual-editor-guest') return;

		const expectedOrigin = this.#getExpectedOrigin();
		if (!expectedOrigin || event.origin !== expectedOrigin) return;

		const expectedSource = this.#getExpectedSource();
		if (!expectedSource || event.source !== expectedSource) return;

		const handler = this.#handlers[data.type as keyof UmbVisualEditorGuestMessageHandlers];
		handler?.(data);
	};
}
