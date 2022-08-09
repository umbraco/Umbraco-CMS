import { css, html, LitElement } from 'lit';
import { customElement, state, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { data } from '../../../mocks/data/node.data';

@customElement('umb-media-section-tree')
class UmbMediaSectionTree extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];

	@property()
	public currentNodeId?: string;

	// simplified tree data for testing
	@state()
	_tree: Array<any> = data.filter((item) => item.type === 'media');

	@state()
	_section?: string;

	render() {
		return html`
			<a href="${'/section/media'}">
				<h3>Media</h3>
			</a>

			<div class="nav-list">
				${this._tree.map(
					(item) => html`
						<uui-menu-item
							?active="${parseInt(this.currentNodeId || '-1') === item.id}"
							label="${item.name}"
							href="/section/media/node/${item.id}">
							<uui-icon slot="icon" name="${item.icon}"></uui-icon>
						</uui-menu-item>
					`
				)}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-section-tree': UmbMediaSectionTree;
	}
}
