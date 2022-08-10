import '../../../backoffice/components/backoffice-modal-container.element';
import '../../../core/services/modal/layouts/content-picker/modal-layout-content-picker.element';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-dialog';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../context';
import { LitElement } from 'lit';
import { UmbModalHandler, UmbModalOptions, UmbModalService } from './';

export default {
	title: 'API/Modals',
	component: 'umb-installer',
	decorators: [
		(story) =>
			html`<umb-context-provider
				style="display: block; padding: 32px;"
				key="umbModalService"
				.value=${new UmbModalService()}>
				${story()}
			</umb-context-provider>`,
	],
	id: 'installer-page',
	argTypes: {
		modalType: { control: 'select', options: ['sidebar', 'dialog'] },
	},
} as Meta;

@customElement('story-modal-service-dialog-example')
class DialogExampleElement extends UmbContextConsumerMixin(LitElement) {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _close() {
		this.modalHandler?.close();
	}

	private _submit(value: any) {
		this.modalHandler?.close(value);
	}

	render() {
		return html`
			<uui-dialog>
				<uui-dialog-layout headline="I am a dialog">
					<p>By clicking the close button, the modal is closed</p>
					<p>
						By clicking the submit button, the modal is closed and the value <b>I am the value</b> is returned to the
						component that opened this modal
					</p>
					<uui-button slot="actions" look="secondary" @click=${() => this._close()} label="close"></uui-button>
					<uui-button
						slot="actions"
						look="primary"
						color="positive"
						@click=${() => this._submit('I am the value')}
						label="submit"></uui-button>
				</uui-dialog-layout>
			</uui-dialog>
		`;
	}
}

@customElement('story-modal-service-sidebar-example')
class SidebarExampleElement extends UmbContextConsumerMixin(LitElement) {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _close() {
		this.modalHandler?.close();
	}

	private _submit(value: any) {
		this.modalHandler?.close(value);
	}

	render() {
		return html`<div style="background: white; padding: 16px">
			<h2>Sidebar</h2>
			<p>By clicking the close button, the modal is closed</p>
			<p>
				By clicking the submit button, the modal is closed and the value <b>I am the value</b> is returned to the
				component that opened this modal
			</p>
			<uui-button look="secondary" @click=${() => this._close()} label="close"></uui-button>
			<uui-button
				look="primary"
				color="positive"
				@click=${() => this._submit('I am the value')}
				label="submit"></uui-button>
		</div>`;
	}
}

@customElement('story-modal-service-example')
class StoryModalServiceExampleElement extends UmbContextConsumerMixin(LitElement) {
	private _modalService?: UmbModalService;

	@property()
	modalType: 'dialog' | 'sidebar' = 'dialog';

	@property()
	modalOptions: UmbModalOptions<unknown> = { type: 'sidebar', size: 'small' };

	@state()
	value = '';

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _open() {
		let modalHandler = null;
		if (this.modalType === 'dialog') {
			modalHandler = this._modalService?.open('story-modal-service-dialog-example');
		}
		if (this.modalType === 'sidebar') {
			modalHandler = this._modalService?.open('story-modal-service-sidebar-example', this.modalOptions);
		}
		modalHandler?.onClose.then((result) => {
			this.value = result ?? this.value;
		});
	}

	render() {
		return html`
			<uui-button label="open-dialog" look="primary" @click=${() => this._open()}>Open modal</uui-button>
			<p style="margin-bottom: 0">The value is: ${this.value}</p>
			<uui-button label="reset" @click=${() => (this.value = '')}></uui-button>
		`;
	}
}

const Template: Story = (props) => {
	return html`
		<umb-backoffice-modal-container></umb-backoffice-modal-container>
		<story-modal-service-example .modalType=${props.modalType}></story-modal-service-example>
	`;
};

export const Dialog = Template.bind({});
Dialog.args = {
	modalType: 'dialog',
};

export const Sidebar = Template.bind({});
Sidebar.args = {
	modalType: 'sidebar',
	modalOptions: { size: 'small' },
};
