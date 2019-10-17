-- https://mariadb.com/kb/en/library/data-types/
-- use kesso;
CREATE TABLE attachment(
	attachment_key varchar(18) NOT NULL,
	attachment_gubun varchar(18) NOT NULL,
	attachment_detail_code varchar(18) NOT NULL,
	file_name varchar(300) NOT NULL,
	file_format varchar(10) NULL,
	file_size bigint NULL,
	thumbnail_path varchar(1000) NULL,
	note varchar(400) NULL,
	operator_key varchar(18) NULL,
	operator_ip varchar(15) NULL,
	input_datetime datetime NULL
)
;
CREATE TABLE calendar(
	calendar_key varchar(18) NOT NULL,
	operator_key varchar(18) NULL,
	startTime varchar(100) NULL,
	endTime varchar(100) NULL,
	summary varchar(200) NULL,
	--description varchar(max) NULL,
    description text NULL,
	calendar varchar(10) NULL,
	is_public char(1) NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
) 
;
CREATE TABLE common_code(
	code_group_key varchar(50) NOT NULL,
	code_key varchar(50) NOT NULL,
	code_label varchar(200) NOT NULL,
	display_order int NOT NULL,
	include_all char(1) NULL,
	is_visible char(1) NULL,
	is_deleted char(1) NULL,
	note varchar(200) NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
) 
;
CREATE TABLE menu_allow_userAuth(
	menu_key varchar(18) NOT NULL,
	user_authority_code varchar(50) NOT NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;
CREATE TABLE menu_allow_userGroup(
	menu_key varchar(18) NOT NULL,
	user_group_code varchar(50) NOT NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;
CREATE TABLE menu(
	menu_key varchar(18) NOT NULL,
	up_menu_key varchar(18) NULL,
	menu_label varchar(50) NOT NULL,
	component_key varchar(200) NULL,
	module_path varchar(500) NULL,
	display_order int NULL,
	note varchar(200) NULL,
	is_using char(1) NULL,
	is_deleted char(1) NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;
CREATE TABLE user_authority(
	user_authority_key varchar(18) NOT NULL,
	user_key varchar(18) NOT NULL,
	target_depart_code varchar(50) NOT NULL,
	user_authority_code varchar(50) NOT NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;
CREATE TABLE user_group(
	user_key varchar(18) NOT NULL,
	user_group_code varchar(50) NOT NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;
CREATE TABLE user(
	user_key varchar(18) NOT NULL,
	user_id varchar(18) NULL,
	password nvarchar(40) NULL,
	name varchar(20) NOT NULL,
	email varchar(100) NULL,
	phone varchar(20) NULL,
	depart_code varchar(50) NULL,
	position_code varchar(50) NULL,
	is_active char(1) NOT NULL,
	is_deleted char(1) NULL,
	input_datetime datetime NULL,
	modify_datetime datetime NULL
)
;