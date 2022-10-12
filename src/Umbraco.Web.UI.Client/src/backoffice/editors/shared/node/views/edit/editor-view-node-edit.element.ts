import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { NodeEntity, NodeProperty, NodePropertyData } from '../../../../../../mocks/data/node.data';
import { UmbContextConsumerMixin } from '../../../../../../core/context';
import { UmbNodeContext } from '../../node.context';

import '../../../../../components/node-property/node-property.element';
import { UmbObserverMixin } from '../../../../../../core/observer';

@customElement('umb-editor-view-node-edit')
export class UmbEditorViewNodeEditElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	@state()
	_properties: NodeProperty[] = [];

	@state()
	_data: NodePropertyData[] = [];

	private _nodeContext?: UmbNodeContext;

	constructor() {
		super();

		this.consumeContext('umbNodeContext', (nodeContext) => {
			this._nodeContext = nodeContext;
			this._observeNode();
		});
	}

	private _observeNode() {
		if (!this._nodeContext) return;

		this.observe<NodeEntity>(this._nodeContext.data.pipe(distinctUntilChanged()), (node) => {
			this._properties = node.properties;
			this._data = node.data;
		});
	}

	render() {
		return html`
			<uui-box>
				${this._properties.map(
					(property: NodeProperty) => html`
						<umb-node-property
							.property=${property}
							.value=${this._data.find((data) => data.alias === property.alias)?.value}></umb-node-property>
					`
				)}
			</uui-box>
		`;
	}
}

export default UmbEditorViewNodeEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-node-edit': UmbEditorViewNodeEditElement;
	}
}
