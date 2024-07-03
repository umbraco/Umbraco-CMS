// import { UMB_UFM_RENDER_CONTEXT } from '@umbraco-cms/backoffice/components';
// import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';
// import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
// import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// const elementName = 'ufm-document-name';

// @customElement(elementName)
// export class UmbUfmDocumentNameElement extends UmbLitElement {
// 	@property()
// 	alias?: string;

// 	@state()
// 	private _value?: unknown;

// 	#documentRepository = new UmbDocumentItemRepository(this);

// 	constructor() {
// 		super();

// 		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
// 			this.observe(
// 				context.value,
// 				async (value) => {
// 					if (!value) return;

// 					const unique =
// 						this.alias && typeof value === 'object'
// 							? ((value as Record<string, unknown>)[this.alias] as string)
// 							: (value as string);

// 					if (!unique) return;

// 					const { data } = await this.#documentRepository.requestItems([unique]);

// 					this._value = data?.[0]?.name;
// 				},
// 				'observeValue',
// 			);
// 		});
// 	}

// 	override render() {
// 		return this._value ?? this.alias;
// 	}
// }

// export { UmbUfmDocumentNameElement as element };

// declare global {
// 	interface HTMLElementTagNameMap {
// 		[elementName]: UmbUfmDocumentNameElement;
// 	}
// }
