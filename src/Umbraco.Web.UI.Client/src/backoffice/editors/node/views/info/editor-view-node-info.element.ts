import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import { UmbNodeContext } from '../../node.context';
import { Subscription, distinctUntilChanged } from 'rxjs';

@customElement('umb-editor-view-node-info')
export class UmbEditorViewNodeInfoElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _nodeContext?: UmbNodeContext;
	private _nodeContextSubscription?: Subscription;

	@state()
	private _nodeName = '';

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
			this._nodeName = node.name;
		});
	}

	render() {
		return html`<div>Info Editor View for ${this._nodeName}</div>`;
	}
}

export default UmbEditorViewNodeInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-node-info': UmbEditorViewNodeInfoElement;
	}
}
