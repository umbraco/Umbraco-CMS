import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
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

	@property()
	type?: StartNode['type'];

	@property()
	query?: string;

	@property({ type: String })
	startNodeId?: string;

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = 0;

	private _filter: Array<string> = [];
	@property()
	public set filter(value: string) {
		this._filter = value.split(',');
	}
	public get filter(): string {
		return this._filter.join(',');
	}

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: Boolean })
	ignoreUserStartNodes?: boolean;

	@property()
	public set value(newValue: string) {
		if (newValue) {
			super.value = newValue;
			this.items = newValue.split(',');
		}
	}
	public get value(): string {
		return super.value as string;
	}

	@state()
	items: Array<string> = [];

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputDocumentElement).selectedIds.join(',');
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
					.startNodeId=${this.startNodeId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?showOpenButton=${this.showOpenButton}
					?ignoreUserStartNodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-document>`;
			case 'media':
				return html`<umb-input-media
					.selectedIds=${this.items}
					.startNodeId=${this.startNodeId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?showOpenButton=${this.showOpenButton}
					?ignoreUserStartNodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-media>`;
			case 'member':
				return html`<umb-input-member
					.selectedIds=${this.items}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?showOpenButton=${this.showOpenButton}
					?ignoreUserStartNodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}>
				</umb-input-member>`;
			default:
				return html`Node type could not be found`;
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
