```sql

-- Create tenant table
CREATE TABLE tenant (
    tenantid VARCHAR(255) PRIMARY KEY,
    adminid VARCHAR(255),
    adminemail VARCHAR(255),
    ad_url VARCHAR(255);

);

-- Create dept table
CREATE TABLE dept (
    deptid VARCHAR(255) PRIMARY KEY,
    hodid VARCHAR(255),
    tenantid VARCHAR(255)
);

-- Create employee table
CREATE TABLE employee (
    empid VARCHAR(255) PRIMARY KEY,
    email VARCHAR(255),
    name VARCHAR(255),
    level VARCHAR(255),
    roleid VARCHAR(255),
    supervisorid VARCHAR(255),
    tenantid VARCHAR(255),
    deptid VARCHAR(255),
    active VARCHAR(255)
);



-- Create roles table
CREATE TABLE roles (
    roleid VARCHAR(255) PRIMARY KEY,
    tenantid VARCHAR(255),
    deptid VARCHAR(255)
);

-- Create permissions table
CREATE TABLE permissions (
    permid VARCHAR(255) PRIMARY KEY,
    roleid VARCHAR(255),
    permissionname VARCHAR(255)
);

-- Create datasource table
CREATE TABLE datasource (
    sourceid VARCHAR(255) PRIMARY KEY,
    deptid VARCHAR(255),
    sourcelink VARCHAR(255),
    comments VARCHAR(255)
);

-- Create kra table
CREATE TABLE kra (
    kraid VARCHAR(255) PRIMARY KEY,
    empid VARCHAR(255),
    date DATE,
    kratype VARCHAR(255),
    reportedto VARCHAR(255)
);

-- Create tasks table
CREATE TABLE tasks (
    taskid VARCHAR(255) PRIMARY KEY,
    tasktype VARCHAR(255),
    kraid VARCHAR(255),
    date DATE
);

-- Create task_scores table
CREATE TABLE task_scores (
    scoreid VARCHAR(255) PRIMARY KEY,
    taskid VARCHAR(255),
    superrating INT,
    automatedrating INT,
    comments VARCHAR(255)
);

-- Create kpi_details table
CREATE TABLE kpi_details (
    kpi_id VARCHAR(255) PRIMARY KEY,
    kpi_name VARCHAR(255),
    kpi_score FLOAT
);

-- Create kpi_criteria table
CREATE TABLE kpi_criteria (
    criteria_id INT PRIMARY KEY,
    level VARCHAR(255),
    tasktype VARCHAR(255),
    count INT,
    no_of_comments INT,
    kpi_id VARCHAR(255)
);

-- Create competencies table
CREATE TABLE competencies (
    compid VARCHAR(255) PRIMARY KEY,
    level VARCHAR(255),
    department VARCHAR(255),
    tenant VARCHAR(255)
);

-- Create competency_score table
CREATE TABLE competency_score (
    compscoreid VARCHAR(255) PRIMARY KEY,
    compid VARCHAR(255),
    taskid VARCHAR(255)
);



```