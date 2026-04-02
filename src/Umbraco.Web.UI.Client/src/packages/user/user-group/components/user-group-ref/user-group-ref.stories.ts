import type { UmbUserGroupRefElement } from './user-group-ref.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './user-group-ref.element.js';

const meta: Meta<UmbUserGroupRefElement> = {
	title: 'Entity/User Group/Components/User Group Ref',
	component: 'umb-user-group-ref',
};

export default meta;
type Story = StoryObj<UmbUserGroupRefElement>;

export const Docs: Story = {
	args: {
		name: 'Administrators',
		documentRootAccess: true,
		mediaRootAccess: true,
	},
};

export const WithSlots: Story = {
	args: {
		name: 'Custom Data Type',
	},
	render: (args) => html`
		<umb-user-group-ref .name=${args.name} .detail=${args.detail}>
			<div slot="actions">
				<uui-action-bar>
					<uui-button label="delete" look="primary" color="danger" compact>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</div>
		</umb-user-group-ref>
	`,
};
