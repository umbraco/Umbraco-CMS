import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbHistoryService } from 'src/core/services/history';

@customElement('umb-section-view-packages-repo')
export class UmbSectionViewPackagesRepoElement extends LitElement {
	/**
	 *
	 */
	constructor() {
		super();
		umbHistoryService.push({ label: 'Packages', path: 'section/packages/view/packages' });
	}
	render() {
		return html`
			<uui-box headline="Popular"></uui-box>
			<uui-box headline="Promoted"></uui-box>
		`;
	}
}

export default UmbSectionViewPackagesRepoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-packages-repo': UmbSectionViewPackagesRepoElement;
	}
}
