import { css, html, LitElement } from 'lit';
import { customElement, state, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { data } from '../../../mocks/data/node.data';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbNodeStore } from '../../../core/stores/node.store';
import { map, Subscription } from 'rxjs';

@customElement('umb-content-section-tree')
class UmbContentSectionTree extends UmbContextConsumerMixin(LitElement) {
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
	_tree: Array<any> = data.filter((item) => item.type === 'document');

	@state()
	_section?: string;

	private _nodeStore?: UmbNodeStore;
	private _nodesSubscription?: Subscription;

	constructor() {
		super();

		// TODO: temp solution until we know where to get tree data from
		this.consumeContext('umbNodeStore', (store) => {
			this._nodeStore = store;

			this._nodesSubscription = this._nodeStore
				?.getAll()
				.pipe(map((nodes) => nodes.filter((node) => node.type === 'media')))
				.subscribe((mediaNodes) => {
					this._tree = mediaNodes;
				});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._nodesSubscription?.unsubscribe();
	}

	render() {
		return html`
			<a href="${'/section/content'}">
				<h3>Content</h3>
			</a>

			<div class="nav-list">
				${this._tree.map(
					(item) => html`
						<uui-menu-item
							?active="${parseInt(this.currentNodeId || '-1') === item.id}"
							label="${item.name}"
							href="/section/content/node/${item.id}">
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
		'umb-content-section-tree': UmbContentSectionTree;
	}
}
