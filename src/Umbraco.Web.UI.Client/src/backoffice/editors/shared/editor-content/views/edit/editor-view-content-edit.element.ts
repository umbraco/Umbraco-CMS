import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { NodeEntity, NodeProperty, NodePropertyData } from '../../../../../../core/mocks/data/node.data';
import { UmbNodeContext } from '../../node.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import '../../../../../components/node-property/node-property.element';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-editor-view-content-edit')
export class UmbEditorViewContentEditElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display:block;
				margin: var(--uui-size-layout-1);
			}
		`
	];

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

export default UmbEditorViewContentEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-content-edit': UmbEditorViewContentEditElement;
	}
}
