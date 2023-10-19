import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';

export type NodeType = 'content' | 'member' | 'media';

export type StartNode = {
	type?: NodeType;
	query?: string | null;
	id?: string | null;
};

@customElement('umb-input-tree')
export class UmbInputTreeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@state()
	type?: StartNode['type'];

	@state()
	query?: string;

	@state()
	startId?: string;

	@state()
	min = 0;

	@state()
	max = 0;

	@state()
	filter?: string;

	@state()
	showOpenButton?: boolean;

	@state()
	ignoreUserStartNodes?: boolean;

	@property({ attribute: false })
	public set configuration(value: UmbPropertyEditorConfigCollection | undefined) {
		const config: Record<string, any> = {
			...(value ? value.toObject() : {}),
		};

		this.type = config.startNode.type.toLowerCase() as StartNode['type'];
		this.query = config.query;
		this.startId = config.startNode.id;

		this.min = config.minNumber;
		this.max = config.maxNumber;

		this.filter = config.filter;
		this.showOpenButton = config.showOpenButton;
		this.ignoreUserStartNodes = config.ignoreUserStartNodes;
	}

	@property()
	set value(newValue: string | string[]) {
		super.value = newValue;
		this.items = newValue.split(',');
	}
	get value(): string {
		return super.value as string;
	}

	@state()
	items: Array<string> = [];

	#onChange(event: CustomEvent) {
		this.items = (event.target as UmbInputDocumentElement).selectedIds;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	constructor() {
		super();
	}

	render() {
		return html`${this.#renderElement()}
			<p>${this.#renderTip()}</p>`;
	}

	#renderTip() {
		if (this.items.length && this.items.length !== this.max) {
			if (this.items.length > this.max) {
				return `You can only have up to ${this.max} item(s) selected`;
			}
			if (this.min === this.max && this.min !== 0) {
				return `Add ${this.min - this.items.length} more item(s)`;
			}
			if (this.min === 0 && this.max) {
				return `Add up to ${this.max} items`;
			}
			if (this.min > 0 && this.max > 0) {
				return `Add between ${this.min} and ${this.max} item(s)`;
			}
			if (this.items.length < this.min) {
				return `You need to add at least ${this.min} items`;
			}
		}
		return '';
	}

	#renderElement() {
		switch (this.type) {
			case 'content':
				return html`<umb-input-document
					.selectedIds=${this.items}
					.query=${this.query}
					.node-start-id=${this.startId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?show-open-button=${this.showOpenButton}
					?ignore-user-start-nodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-document>`;
			case 'media':
				return html`<umb-input-media
					.selectedIds=${this.items}
					.node-start-id=${this.startId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?show-open-button=${this.showOpenButton}
					?ignore-user-start-nodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-media>`;
			case 'member':
				return html`<umb-input-member
					.selectedIds=${this.items}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?show-open-button=${this.showOpenButton}
					?ignore-user-start-nodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}>
				</umb-input-member>`;
			default:
				return html`Type not found`;
		}
	}

	static styles = [
		css`
			p {
				margin: 0;
				color: var(--uui-color-border-emphasis);
			}
		`,
	];
}

export default UmbInputTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tree': UmbInputTreeElement;
	}
}
