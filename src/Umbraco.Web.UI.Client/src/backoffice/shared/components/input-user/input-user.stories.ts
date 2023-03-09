import { Meta, StoryObj } from '@storybook/web-components';
import './input-user.element';
import type { UmbPickerUserElement } from './input-user.element';

const meta: Meta<UmbPickerUserElement> = {
    title: 'Components/Inputs/User',
    component: 'umb-input-user',
    argTypes: {
        modalType: {
            control: 'inline-radio',
            options: ['dialog', 'sidebar'],
            defaultValue: 'sidebar',
            description: 'The type of modal to use when selecting users',
        },
        modalSize:{
            control: 'select',
            options: ['small', 'medium', 'large', 'full'],
            defaultValue: 'small',
            description: 'The size of the modal to use when selecting users, only applicable to sidebar not dialog',
        }
    }
};
  
export default meta;
type Story = StoryObj<UmbPickerUserElement>;
  
export const Overview: Story = {
    args: {
    }
};