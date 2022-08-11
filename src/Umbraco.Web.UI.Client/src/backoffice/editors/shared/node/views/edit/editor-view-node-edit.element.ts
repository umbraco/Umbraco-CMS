import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { NodeProperty, NodePropertyData } from '../../../../../../mocks/data/node.data';
import { UmbContextConsumerMixin } from '../../../../../../core/context';
import { UmbNodeContext } from '../../node.context';
import { Subscription, distinctUntilChanged } from 'rxjs';

import '../../../../../components/node-property.element';

@customElement('umb-editor-view-node-edit')
export class UmbEditorViewNodeEditElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			hr {
				border: 0;
				border-top: 1px solid var(--uui-color-border);
			}
		`,
	];

	@state()
	_properties: NodeProperty[] = [];

	@state()
	_data: NodePropertyData[] = [];

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
		this._nodeContextSubscription?.unsubscribe();

		this._nodeContextSubscription = this._nodeContext?.data.pipe(distinctUntilChanged()).subscribe((node) => {
			this._properties = node.properties;
			this._data = node.data;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._nodeContextSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box>
				${this._properties.map(
					(property: NodeProperty) => html`
						<umb-node-property
							.property=${property}
							.value=${this._data.find((data) => data.alias === property.alias)?.value}></umb-node-property>
						<hr />
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
