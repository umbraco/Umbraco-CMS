import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { repeat } from 'lit/directives/repeat.js';
import { UmbLitElement } from '@umbraco-cms/element';
import './template-card.element';
import UmbTemplateCardElement from './template-card.element';

export interface TemplateModel {
	name: string;
	key: string;
}

@customElement('umb-template-card-list')
export class UmbTemplateCardListElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				box-sizing: border-box;
				display: flex;
				align-items: stretch;
				gap: var(--uui-size-space-4);
				min-height: 180px;
			}
		`,
	];

	@property({ type: Array<TemplateModel> })
	templates: TemplateModel[] = [
		{
			name: 'Named template',
			key: '123',
		},
		{
			name: 'Named template 2',
			key: '456',
		},
	];

	private _selected?: number;

	private _templateCardElements: UmbTemplateCardElement[] = [];

	protected getFormElement() {
		return undefined;
	}

	#changeSelectedState(e: UmbTemplateCardElement) {
		const newValue = e.value;
		this._templateCardElements.forEach((el, index) => {
			if ((el.value as string) === newValue) {
				el.selected = true;
				this._selected = index;
			} else {
				el.selected = false;
			}
		});
		this.value = newValue;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#delete(e: Event) {
		const key = (e.target as UmbTemplateCardElement).value;
		const i = this.templates.findIndex((x) => x.key === key);
		this.templates.splice(i, 1);
		this.templates = [...this.templates];
	}

	#openTemplate(e: Event) {
		console.log(e.target);
		const key = (e.target as UmbTemplateCardElement).value;
		console.log('open', key);
	}

	#openTemplatePicker() {
		console.log('template picker');
	}

	#slotchange(e: Event) {
		this._templateCardElements.forEach((el) => {
			el.removeEventListener('selected', (e) => this.#changeSelectedState(e.target as UmbTemplateCardElement));
			el.removeEventListener('open', (e) => this.#openTemplate(e));
		});

		this._templateCardElements = (e.target as HTMLSlotElement)
			.assignedElements({ flatten: true })
			.filter((el) => el instanceof UmbTemplateCardElement) as UmbTemplateCardElement[];

		if (this._templateCardElements.length === 0) return;

		this._templateCardElements.forEach((el) => {
			el.addEventListener('selected', (e) => this.#changeSelectedState(e.target as UmbTemplateCardElement));
			el.addEventListener('open', (e) => this.#openTemplate(e));
		});
	}

	render() {
		return html` <slot @slotchange="${this.#slotchange}"></slot> `;
	}
}

export default UmbTemplateCardListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-card-list': UmbTemplateCardListElement;
	}
}
