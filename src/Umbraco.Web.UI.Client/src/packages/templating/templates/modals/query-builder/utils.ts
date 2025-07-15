import type {
	TemplateQueryExecuteSortModel,
	TemplateQueryOperatorModel,
	TemplateQueryPropertyPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	OperatorModel,
	TemplateQueryPropertyTypeModel,
	UserOrderModel,
} from '@umbraco-cms/backoffice/external/backend-api';

type TemplateOperatorModel = TemplateQueryOperatorModel & { localizeKey?: string };
type TemplatePropertyModel = TemplateQueryPropertyPresentationModel & { localizeKey?: string };
type TemplateSortModel = TemplateQueryExecuteSortModel & { localizeKey?: string };

/**
 *
 * @param operators
 * @param currentPropertyType
 */
export function localizeOperators(
	operators: Array<TemplateQueryOperatorModel>,
	currentPropertyType: TemplateQueryPropertyTypeModel | null,
): Array<TemplateOperatorModel> {
	switch (currentPropertyType) {
		case TemplateQueryPropertyTypeModel.STRING:
			return isString(operators);
		case TemplateQueryPropertyTypeModel.INTEGER:
			return isInteger(operators);
		case TemplateQueryPropertyTypeModel.DATE_TIME:
			return isDateTime(operators);
		default:
			return operators;
	}
}

/**
 *
 * @param propertyTypes
 */
export function localizePropertyType(propertyTypes?: Array<TemplateQueryPropertyPresentationModel>) {
	if (!propertyTypes) return;
	return propertyTypes.map((propertyType): TemplatePropertyModel => {
		switch (propertyType.alias) {
			case UserOrderModel.NAME:
				return { ...propertyType, localizeKey: 'template_name' };
			case UserOrderModel.ID:
				return { ...propertyType, localizeKey: 'template_id' };
			case UserOrderModel.CREATE_DATE:
				return { ...propertyType, localizeKey: 'template_createdDate' };
			case UserOrderModel.UPDATE_DATE:
				return { ...propertyType, localizeKey: 'template_lastUpdatedDate' };
			default:
				return propertyType;
		}
	});
}

/**
 *
 * @param sort
 */
export function localizeSort(sort?: TemplateQueryExecuteSortModel | null): TemplateSortModel | undefined {
	if (!sort?.direction) return undefined;
	switch (sort.direction) {
		case 'ascending':
			return { ...sort, localizeKey: 'template_ascending' };
		case 'descending':
			return { ...sort, localizeKey: 'template_descending' };
		default:
			return sort;
	}
}

// Following code is for localization of operators (checks on property type);

/**
 *
 * @param operators
 */
function isString(operators: Array<TemplateQueryOperatorModel>): Array<TemplateOperatorModel> {
	return operators.map((operator): TemplateOperatorModel => {
		switch (operator.operator) {
			case OperatorModel.EQUALS:
				return { ...operator, localizeKey: 'template_is' };
			case OperatorModel.NOT_EQUALS:
				return { ...operator, localizeKey: 'template_isNot' };
			case OperatorModel.CONTAINS:
				return { ...operator, localizeKey: 'template_contains' };
			case OperatorModel.NOT_CONTAINS:
				return { ...operator, localizeKey: 'template_doesNotContain' };
			default:
				return operator;
		}
	});
}

/**
 *
 * @param operators
 */
function isInteger(operators: Array<TemplateQueryOperatorModel>): Array<TemplateOperatorModel> {
	return operators.map((operator): TemplateOperatorModel => {
		switch (operator.operator) {
			case OperatorModel.EQUALS:
				return { ...operator, localizeKey: 'template_equals' };
			case OperatorModel.NOT_EQUALS:
				return { ...operator, localizeKey: 'template_doesNotEqual' };
			case OperatorModel.GREATER_THAN:
				return { ...operator, localizeKey: 'template_greaterThan' };
			case OperatorModel.GREATER_THAN_EQUAL_TO:
				return { ...operator, localizeKey: 'template_greaterThanEqual' };
			case OperatorModel.LESS_THAN:
				return { ...operator, localizeKey: 'template_lessThan' };
			case OperatorModel.LESS_THAN_EQUAL_TO:
				return { ...operator, localizeKey: 'template_lessThanEqual' };
			default:
				return operator;
		}
	});
}

/**
 *
 * @param operators
 */
function isDateTime(operators: Array<TemplateQueryOperatorModel>): Array<TemplateOperatorModel> {
	return operators.map((operator): TemplateOperatorModel => {
		switch (operator.operator) {
			case OperatorModel.GREATER_THAN:
				return { ...operator, localizeKey: 'template_before' };
			case OperatorModel.GREATER_THAN_EQUAL_TO:
				return { ...operator, localizeKey: 'template_beforeIncDate' };
			case OperatorModel.LESS_THAN:
				return { ...operator, localizeKey: 'template_after' };
			case OperatorModel.LESS_THAN_EQUAL_TO:
				return { ...operator, localizeKey: 'template_afterIncDate' };
			default:
				return operator;
		}
	});
}
