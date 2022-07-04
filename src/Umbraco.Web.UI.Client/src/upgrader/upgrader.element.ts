import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbContextProviderMixin } from '../core/context';

@customElement('umb-upgrader')
export class UmbUpgrader extends UmbContextProviderMixin(LitElement) {
	static styles: CSSResultGroup = [css``];

	@state()
	step = 1;

	connectedCallback(): void {
		super.connectedCallback();
		this.addEventListener('next', () => this._handleNext());
		this.addEventListener('previous', () => this._goToPreviousStep());
	}

	private _handleNext() {
		this.step++;
	}

	private _goToPreviousStep() {
		this.step--;
	}

	render() {
		return html`<h1>Please implement me</h1>`;
	}
}

export default UmbUpgrader;

declare global {
	interface HTMLElementTagNameMap {
		'umb-upgrader': UmbUpgrader;
	}
}
