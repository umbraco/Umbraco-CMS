import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { NodeEntity, NodeProperty } from '../../../../mocks/data/content.data';
import { UmbContextConsumerMixin } from '../../../../core/context';
import { UmbNodeContext } from '../node.context';
import { Subscription, distinctUntilChanged } from 'rxjs';

@customElement('umb-editor-view-node-edit')
export class UmbEditorViewNodeEdit extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			hr {
				border: 0;
				/* TODO: Use correct color property */
				border-top: 1px solid #e7e7e7;
			}
		`,
	];

	@state()
	_node?: NodeEntity;

	private _nodeContext?: UmbNodeContext;
	private _nodeContextSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbNodeContext', (nodeContext) => {
			this._nodeContext = nodeContext;
			this._useNode();
		});
	}

	private _useNode() {
		this._nodeContextSubscription = this._nodeContext?.data.pipe(distinctUntilChanged()).subscribe((data) => {
			this._node = data;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._nodeContextSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box>
				${this._node?.properties.map(
					(property: NodeProperty) => html`
						<umb-node-property
							.property=${property}
							.value=${this._node?.data.find((data) => data.alias === property.alias)?.value}></umb-node-property>
						<hr />
					`
				)}
			</uui-box>
		`;
	}
}

export default UmbEditorViewNodeEdit;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-node-edit': UmbEditorViewNodeEdit;
	}
}
