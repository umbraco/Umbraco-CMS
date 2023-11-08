import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

type NodeType = 'content' | 'member' | 'media';
export type StartNode = {
	type?: NodeType;
	id?: string | null;
};

@customElement('umb-input-tree')
export class UmbInputTreeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _type: StartNode['type'] = undefined;
	@property()
	public set type(newType: StartNode['type']) {
		if (newType?.toLowerCase() !== this._type) {
			this._type = newType?.toLowerCase() as StartNode['type'];
		}
	}
	public get type(): StartNode['type'] {
		return this._type;
	}

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
		super.value = newValue;
		if (newValue) {
			this.selectedIds = newValue.split(',');
		} else {
			this.selectedIds = [];
		}
	}
	public get value(): string {
		return super.value as string;
	}

	selectedIds: Array<string> = [];

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputDocumentElement).selectedIds.join(',');
		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();
	}

	render() {
		switch (this.type) {
			case 'content':
				return html`<umb-input-document
					.selectedIds=${this.selectedIds}
					.startNodeId=${this.startNodeId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?showOpenButton=${this.showOpenButton}
					?ignoreUserStartNodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-document>`;
			case 'media':
				return html`<umb-input-media
					.selectedIds=${this.selectedIds}
					.startNodeId=${this.startNodeId}
					.filter=${this.filter}
					.min=${this.min}
					.max=${this.max}
					?showOpenButton=${this.showOpenButton}
					?ignoreUserStartNodes=${this.ignoreUserStartNodes}
					@change=${this.#onChange}></umb-input-media>`;
			case 'member':
				return html`<umb-input-member
					.selectedIds=${this.selectedIds}
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
