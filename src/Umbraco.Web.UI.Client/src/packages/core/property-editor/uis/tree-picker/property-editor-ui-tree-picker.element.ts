import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	type UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbInputTreeElement } from '@umbraco-cms/backoffice/tree';
import type { UmbTreePickerSource } from '@umbraco-cms/backoffice/components';
import { UmbDynamicRootRepository } from '@umbraco-cms/backoffice/dynamic-root';

/**
 * @element umb-property-editor-ui-tree-picker
 */

@customElement('umb-property-editor-ui-tree-picker')
export class UmbPropertyEditorUITreePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	type?: UmbTreePickerSource['type'];

	@state()
	startNodeId?: string | null;

	@state()
	min = 0;

	@state()
	max = 0;

	@state()
	allowedContentTypeIds?: string | null;

	@state()
	showOpenButton?: boolean;

	@state()
	ignoreUserStartNodes?: boolean;

	#dynamicRoot?: UmbTreePickerSource['dynamicRoot'] | undefined;

	#dynamicRootRepository: UmbDynamicRootRepository;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const startNode: UmbTreePickerSource | undefined = config?.getValueByAlias('startNode');
		if (startNode) {
			this.type = startNode.type;
			this.startNodeId = startNode.id;
			this.#dynamicRoot = startNode.dynamicRoot;
		}

		this.min = Number(config?.getValueByAlias('minNumber')) || 0;
		this.max = Number(config?.getValueByAlias('maxNumber')) || 0;

		this.allowedContentTypeIds = config?.getValueByAlias('filter');
		this.showOpenButton = config?.getValueByAlias('showOpenButton');
		this.ignoreUserStartNodes = config?.getValueByAlias('ignoreUserStartNodes');
	}

	constructor() {
		super();

		this.#dynamicRootRepository = new UmbDynamicRootRepository(this);
	}

	connectedCallback() {
		super.connectedCallback();

		// TODO: Implement `startNode.dynamicRoot` [LK]
		if (this.#dynamicRoot) {
			console.log('dynamicRoot', this.#dynamicRoot);

			this.#dynamicRootRepository
				.postDynamicRootQuery({
					context: {
						id: 'b327ffa1-749a-4278-aa63-c020dba6b932',
						parentId: 'b327ffa1-749a-4278-aa63-c020dba6b932',
						culture: null,
						segment: null,
					},
					query: {
						origin: {
							alias: this.#dynamicRoot.originAlias,
							key: this.#dynamicRoot.originKey,
						},
						steps: this.#dynamicRoot.querySteps!.map((step) => {
							return {
								alias: step.alias!,
								documentTypeIds: step.anyOfDocTypeKeys!,
							};
						}),
					},
				})
				.then((result) => {
					// TODO: Implement the result from `postDynamicRootQuery`. [LK]
					console.log('postDynamicRootQuery', result);
				});
		}
	}

	#onChange(e: CustomEvent) {
		this.value = (e.target as UmbInputTreeElement).value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-tree
			.value=${this.value}
			.type=${this.type ?? 'content'}
			.startNodeId=${this.startNodeId ?? ''}
			.min=${this.min}
			.max=${this.max}
			.allowedContentTypeIds=${this.allowedContentTypeIds ?? ''}
			?showOpenButton=${this.showOpenButton}
			?ignoreUserStartNodes=${this.ignoreUserStartNodes}
			@change=${this.#onChange}></umb-input-tree>`;
	}
	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker': UmbPropertyEditorUITreePickerElement;
	}
}
