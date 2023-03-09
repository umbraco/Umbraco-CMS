import { Meta, StoryObj } from '@storybook/web-components';
import './input-user-group.element';
import type { UmbInputPickerUserGroupElement } from './input-user-group.element';

const meta: Meta<UmbInputPickerUserGroupElement> = {
    title: 'Components/Inputs/User Group',
    component: 'umb-input-user-group',
    argTypes: {
        modalType: {
            control: 'inline-radio',
            options: ['dialog', 'sidebar'],
            defaultValue: 'sidebar',
            description: 'The type of modal to use when selecting user groups',
        },
        modalSize:{
            control: 'select',
            options: ['small', 'medium', 'large', 'full'],
            defaultValue: 'small',
            description: 'The size of the modal to use when selecting user groups, only applicable to sidebar not dialog',
        }
    }
};
  
export default meta;
type Story = StoryObj<UmbInputPickerUserGroupElement>;
  
export const Overview: Story = {
    args: {
    }
};