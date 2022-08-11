import '../../../backoffice/components/backoffice-modal-container.element';
import '../../../core/services/modal/layouts/content-picker/modal-layout-content-picker.element';
import '../../context/context-provider.element';
import '../../../backoffice/editors/shared/editor-layout/editor-layout.element';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-dialog';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../context';
import { LitElement } from 'lit';
import { UmbModalService } from './';

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
		modalLayout: { control: 'select', options: ['Confirm', 'Content Picker'] },
	},
} as Meta;

@customElement('story-modal-service-example')
class StoryModalServiceExampleElement extends UmbContextConsumerMixin(LitElement) {
	@property()
	modalLayout = 'confirm';

	@state()
	value = '';

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _open() {
		switch (this.modalLayout) {
			case 'Content Picker':
				this._modalService?.contentPicker();
				break;
			default:
				this._modalService?.confirm({
					headline: 'Headline',
					content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit',
				});
				break;
		}
	}

	render() {
		return html`
			<uui-button label="open-dialog" look="primary" @click=${() => this._open()} style="margin-right: 9px;"
				>Open modal</uui-button
			>
		`;
	}
}

const Template: Story = (props) => {
	return html`
		<umb-backoffice-modal-container></umb-backoffice-modal-container>
		<story-modal-service-example .modalLayout=${props.modalLayout}></story-modal-service-example>
	`;
};

export const Overview = Template.bind({});
